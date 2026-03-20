using techviet_be.Entity;

namespace techviet_be.Repository;

public interface ICartRepository : IGenericRepository<Cart>
{
    Task<Cart?> GetCartWithItemsAsync(Guid userId);
    Task<Cart> GetOrCreateCartAsync(Guid userId);
    Task<CartItem?> GetCartItemByIdAsync(Guid cartItemId, Guid userId);
    Task<IEnumerable<CartItem>> GetCartItemsAsync(Guid cartId, bool asNoTracking = true);
    Task AddCartItemAsync(CartItem item);
    void UpdateCartItem(CartItem item);
    void DeleteCartItem(CartItem item);
}
