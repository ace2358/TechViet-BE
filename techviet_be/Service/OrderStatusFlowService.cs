using techviet_be.Entity;

namespace techviet_be.Service;

public interface IOrderStatusFlowService
{
    bool CanTransition(OrderStatus currentStatus, OrderStatus nextStatus);
    void EnsureValidTransition(OrderStatus currentStatus, OrderStatus nextStatus);
}

public class OrderStatusFlowService : IOrderStatusFlowService
{
    private static readonly HashSet<(OrderStatus From, OrderStatus To)> AllowedTransitions =
    [
        (OrderStatus.PENDING, OrderStatus.PAID),
        (OrderStatus.PAID, OrderStatus.SHIPPING),
        (OrderStatus.SHIPPING, OrderStatus.COMPLETED),
        (OrderStatus.PENDING, OrderStatus.CANCELLED)
    ];

    public bool CanTransition(OrderStatus currentStatus, OrderStatus nextStatus)
    {
        if (currentStatus == nextStatus)
        {
            return true;
        }

        if (currentStatus is OrderStatus.COMPLETED or OrderStatus.CANCELLED)
        {
            return false;
        }

        return AllowedTransitions.Contains((currentStatus, nextStatus));
    }

    public void EnsureValidTransition(OrderStatus currentStatus, OrderStatus nextStatus)
    {
        if (!CanTransition(currentStatus, nextStatus))
        {
            throw new InvalidOperationException($"Invalid order status transition: {currentStatus} -> {nextStatus}");
        }
    }
}