using techviet_be.Common;
using techviet_be.Dto;
using techviet_be.Entity;
using techviet_be.Repository;

namespace techviet_be.Service;

public class OrderService(
    IOrderRepository orderRepository,
    IProductRepository productRepository,
    ICartRepository cartRepository,
    IOrderStatusFlowService orderStatusFlowService) : IOrderService
{
    public async Task<Guid> CreateOrderAsync(Guid userId, CreateOrderDto dto)
    {
        if (dto.ShippingFee < 0)
        {
            throw new InvalidOperationException("Shipping fee must be greater than or equal to 0.");
        }

        // 1) Load cart with items + related products
        var cart = await cartRepository.GetCartWithItemsAsync(userId)
            ?? throw new InvalidOperationException("Cart not found.");

        // 2) Validate cart is not empty
        if (cart.Items.Count == 0)
        {
            throw new InvalidOperationException("Cart is empty.");
        }

        await using var transaction = await orderRepository.BeginTransactionAsync();

        try
        {
            decimal subtotal = 0;
            var orderItems = new List<OrderItem>();

            foreach (var item in cart.Items)
            {
                if (item.Quantity <= 0)
                {
                    throw new InvalidOperationException("Quantity must be greater than 0.");
                }

                // 3) Validate stock per item (never trust client)
                var product = await productRepository.GetByIdAsync(item.ProductId)
                    ?? throw new KeyNotFoundException($"Product not found: {item.ProductId}");

                if (!product.IsActive)
                {
                    throw new InvalidOperationException($"Product is inactive: {product.Name}");
                }

                if (item.Quantity > product.StockQty)
                {
                    throw new InvalidOperationException($"Insufficient stock for product: {product.Name}");
                }

                // 4,6) Calculate totals + copy cart item to order item with price snapshot
                subtotal += product.Price * item.Quantity;
                orderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = product.Price
                });

                // 7) Deduct stock
                product.StockQty -= item.Quantity;
                productRepository.Update(product);
            }

            // 5) Create order with pending statuses
            var order = new Order
            {
                UserId = userId,
                Fullname = dto.Fullname,
                Phone = dto.Phone,
                Address = dto.Address,
                Note = dto.Note,
                PaymentMethod = dto.PaymentMethod,
                PaymentStatus = PaymentStatus.PENDING,
                OrderStatus = OrderStatus.PENDING,
                Subtotal = subtotal,
                ShippingFee = dto.ShippingFee,
                Total = subtotal + dto.ShippingFee,
                Items = orderItems
            };

            await orderRepository.AddAsync(order);

            // 8) Clear cart items
            foreach (var cartItem in cart.Items)
            {
                var trackedItem = await cartRepository.GetCartItemByIdAsync(cartItem.Id, userId);
                if (trackedItem is not null)
                {
                    cartRepository.DeleteCartItem(trackedItem);
                }
            }

            await orderRepository.SaveChangesAsync();

            // 9,10) Commit and return order id
            await transaction.CommitAsync();
            return order.Id;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<IEnumerable<OrderResponseDto>> GetOrdersByUserAsync(Guid userId)
    {
        var orders = await orderRepository.GetOrdersByUserAsync(userId);

        return orders.Select(order => new OrderResponseDto
        {
            Id = order.Id,
            CreatedAt = order.CreatedAt,
            PaymentStatus = order.PaymentStatus,
            OrderStatus = order.OrderStatus,
            Subtotal = order.Subtotal,
            ShippingFee = order.ShippingFee,
            Total = order.Total,
            Items = order.Items.Select(item => new OrderItemResponseDto
            {
                ProductId = item.ProductId,
                ProductName = item.Product?.Name ?? string.Empty,
                Quantity = item.Quantity,
                Price = item.Price
            }).ToList()
        });
    }

    public async Task<PagedResult<OrderResponseDto>> GetOrdersByUserAsync(Guid userId, PaginationParams pagination)
    {
        var (orders, totalCount) = await orderRepository.GetOrdersByUserAsync(userId, pagination);

        return new PagedResult<OrderResponseDto>
        {
            Data = orders.Select(order => new OrderResponseDto
            {
                Id = order.Id,
                CreatedAt = order.CreatedAt,
                PaymentStatus = order.PaymentStatus,
                OrderStatus = order.OrderStatus,
                Subtotal = order.Subtotal,
                ShippingFee = order.ShippingFee,
                Total = order.Total,
                Items = order.Items.Select(item => new OrderItemResponseDto
                {
                    ProductId = item.ProductId,
                    ProductName = item.Product?.Name ?? string.Empty,
                    Quantity = item.Quantity,
                    Price = item.Price
                }).ToList()
            }),
            TotalCount = totalCount,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize
        };
    }

    public async Task<OrderResponseDto> UpdateOrderStatusAsync(Guid orderId, string status)
    {
        if (!Enum.TryParse<OrderStatus>(status, true, out var nextStatus))
        {
            throw new InvalidOperationException("Invalid order status.");
        }

        var order = await orderRepository.GetOrderWithItemsAsync(orderId)
            ?? throw new KeyNotFoundException("Order not found.");

        var currentStatus = order.OrderStatus;
        orderStatusFlowService.EnsureValidTransition(currentStatus, nextStatus);

        if (nextStatus == OrderStatus.PAID)
        {
            order.PaymentStatus = PaymentStatus.PAID;
        }

        if (nextStatus == OrderStatus.CANCELLED)
        {
            foreach (var item in order.Items)
            {
                var product = await productRepository.GetByIdAsync(item.ProductId);
                if (product is not null)
                {
                    product.StockQty += item.Quantity;
                    productRepository.Update(product);
                }
            }
        }

        order.OrderStatus = nextStatus;
        orderRepository.Update(order);
        await orderRepository.SaveChangesAsync();

        return new OrderResponseDto
        {
            Id = order.Id,
            CreatedAt = order.CreatedAt,
            PaymentStatus = order.PaymentStatus,
            OrderStatus = order.OrderStatus,
            Subtotal = order.Subtotal,
            ShippingFee = order.ShippingFee,
            Total = order.Total,
            Items = order.Items.Select(item => new OrderItemResponseDto
            {
                ProductId = item.ProductId,
                ProductName = item.Product?.Name ?? string.Empty,
                Quantity = item.Quantity,
                Price = item.Price
            }).ToList()
        };
    }
}
