using System.Text.Json;
using techviet_be.Dto;
using techviet_be.Entity;
using techviet_be.Repository;

namespace techviet_be.Service;

public class CartService(ICartRepository cartRepository, IProductRepository productRepository) : ICartService
{
    public async Task<CartResponseDto> GetCartAsync(Guid userId)
    {
        var cart = await cartRepository.GetOrCreateCartAsync(userId);
        var fullCart = await cartRepository.GetCartWithItemsAsync(userId)
            ?? throw new InvalidOperationException("Cannot load cart.");

        return MapCart(fullCart);
    }

    public async Task<CartResponseDto> AddItemAsync(Guid userId, AddCartItemDto dto)
    {
        if (dto.Quantity <= 0)
        {
            throw new InvalidOperationException("Quantity must be greater than 0.");
        }

        var product = await productRepository.GetByIdAsync(dto.ProductId)
            ?? throw new KeyNotFoundException("Product not found.");

        if (!product.IsActive)
        {
            throw new InvalidOperationException("Product is inactive.");
        }

        if (dto.Quantity > product.StockQty)
        {
            throw new InvalidOperationException("Insufficient stock.");
        }

        var cart = await cartRepository.GetOrCreateCartAsync(userId);

        var currentItems = await cartRepository.GetCartItemsAsync(cart.Id, asNoTracking: false);
        var sameItem = currentItems.FirstOrDefault(x =>
            x.ProductId == dto.ProductId && NormalizeJson(x.VariantInfo) == NormalizeJson(dto.VariantInfo));

        if (sameItem is null)
        {
            var newItem = new CartItem
            {
                CartId = cart.Id,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity,
                VariantInfo = ParseJson(dto.VariantInfo)
            };
            await cartRepository.AddCartItemAsync(newItem);
        }
        else
        {
            var nextQty = sameItem.Quantity + dto.Quantity;
            if (nextQty > product.StockQty)
            {
                throw new InvalidOperationException("Insufficient stock.");
            }

            sameItem.Quantity = nextQty;
            sameItem.VariantInfo = ParseJson(dto.VariantInfo);
            cartRepository.UpdateCartItem(sameItem);
        }

        await cartRepository.SaveChangesAsync();
        var fullCart = await cartRepository.GetCartWithItemsAsync(userId)
            ?? throw new InvalidOperationException("Cannot load cart.");
        return MapCart(fullCart);
    }

    public async Task<CartResponseDto> UpdateItemAsync(Guid userId, Guid cartItemId, UpdateCartItemDto dto)
    {
        if (dto.Quantity <= 0)
        {
            throw new InvalidOperationException("Quantity must be greater than 0.");
        }

        var cartItem = await cartRepository.GetCartItemByIdAsync(cartItemId, userId)
            ?? throw new KeyNotFoundException("Cart item not found.");

        if (dto.Quantity > cartItem.Product.StockQty)
        {
            throw new InvalidOperationException("Insufficient stock.");
        }

        cartItem.Quantity = dto.Quantity;
        cartItem.VariantInfo = ParseJson(dto.VariantInfo);
        cartRepository.UpdateCartItem(cartItem);
        await cartRepository.SaveChangesAsync();

        var fullCart = await cartRepository.GetCartWithItemsAsync(userId)
            ?? throw new InvalidOperationException("Cannot load cart.");
        return MapCart(fullCart);
    }

    public async Task RemoveItemAsync(Guid userId, Guid cartItemId)
    {
        var cartItem = await cartRepository.GetCartItemByIdAsync(cartItemId, userId)
            ?? throw new KeyNotFoundException("Cart item not found.");

        cartRepository.DeleteCartItem(cartItem);
        await cartRepository.SaveChangesAsync();
    }

    private static CartResponseDto MapCart(Cart cart)
    {
        return new CartResponseDto
        {
            CartId = cart.Id,
            UserId = cart.UserId,
            Items = cart.Items.Select(x => new CartItemDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                ProductName = x.Product?.Name,
                Price = x.Product?.Price,
                Quantity = x.Quantity,
                VariantInfo = NormalizeJson(x.VariantInfo)
            }).ToList()
        };
    }

    private static JsonDocument? ParseJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonDocument.Parse(json);
    }

    private static string? NormalizeJson(object? value)
    {
        return value switch
        {
            null => null,
            JsonDocument doc => doc.RootElement.GetRawText(),
            string str => string.IsNullOrWhiteSpace(str) ? null : JsonDocument.Parse(str).RootElement.GetRawText(),
            _ => value.ToString()
        };
    }

    private static string? NormalizeJson(JsonDocument? value)
    {
        return value?.RootElement.GetRawText();
    }

}
