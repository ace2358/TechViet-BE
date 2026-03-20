using techviet_be.Dto;

namespace techviet_be.Service;

public interface ICartService
{
    Task<CartResponseDto> GetCartAsync(Guid userId);
    Task<CartResponseDto> AddItemAsync(Guid userId, AddCartItemDto dto);
    Task<CartResponseDto> UpdateItemAsync(Guid userId, Guid cartItemId, UpdateCartItemDto dto);
    Task RemoveItemAsync(Guid userId, Guid cartItemId);
}
