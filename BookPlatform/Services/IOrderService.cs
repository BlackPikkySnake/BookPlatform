using BookPlatform.DTOs;

namespace BookPlatform.Services
{
    public interface IOrderService
    {
        Task<OrderDto?> GetCartAsync(int userId);
        Task<OrderDto> AddToCartAsync(int userId, AddToCartDto addToCartDto);
        Task<OrderDto> RemoveFromCartAsync(int userId, int orderItemId);
        Task<OrderDto> CheckoutAsync(int userId, CheckoutDto checkoutDto);
        Task<IEnumerable<OrderDto>> GetUserOrdersAsync(int userId);
        Task<OrderDto?> GetOrderByIdAsync(int orderId, int userId);
    }
}