using Microsoft.EntityFrameworkCore;
using techviet_be.Entity;

namespace techviet_be.Repository;

public class CartRepository(AppDbContext context) : GenericRepository<Cart>(context), ICartRepository
{
    public async Task<Cart?> GetCartWithItemsAsync(Guid userId)
    {
        return await Context.Carts
            .AsNoTracking()
            .Include(x => x.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(x => x.UserId == userId);
    }

    public async Task<Cart> GetOrCreateCartAsync(Guid userId)
    {
        var cart = await Context.Carts
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (cart is not null)
        {
            return cart;
        }

        var newCart = new Cart
        {
            UserId = userId
        };

        await Context.Carts.AddAsync(newCart);
        await Context.SaveChangesAsync();

        return newCart;
    }

    public async Task<CartItem?> GetCartItemByIdAsync(Guid cartItemId, Guid userId)
    {
        return await Context.CartItems
            .Include(x => x.Cart)
            .Include(x => x.Product)
            .FirstOrDefaultAsync(x => x.Id == cartItemId && x.Cart.UserId == userId);
    }

    public async Task<IEnumerable<CartItem>> GetCartItemsAsync(Guid cartId, bool asNoTracking = true)
    {
        var query = Context.CartItems.Where(x => x.CartId == cartId);
        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.ToListAsync();
    }

    public async Task AddCartItemAsync(CartItem item)
    {
        await Context.CartItems.AddAsync(item);
    }

    public void UpdateCartItem(CartItem item)
    {
        Context.CartItems.Update(item);
    }

    public void DeleteCartItem(CartItem item)
    {
        Context.CartItems.Remove(item);
    }
}