using Microsoft.EntityFrameworkCore;
using BookPlatform.Data;
using BookPlatform.Models;
using BookPlatform.DTOs;

namespace BookPlatform.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;

        public OrderService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<OrderDto?> GetCartAsync(int userId)
        {
            var cart = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Book)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.UserId == userId && o.Status == OrderStatus.InCart);

            if (cart == null)
                return null;

            return MapToOrderDto(cart);
        }

        public async Task<OrderDto> AddToCartAsync(int userId, AddToCartDto addToCartDto)
        {
            var book = await _context.Books.FindAsync(addToCartDto.BookId);
            if (book == null)
                throw new Exception("Book not found");

            if (book.Status != BookStatus.Published)
                throw new Exception("Cannot add unpublished book to cart");

            // Find existing cart
            var cart = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.UserId == userId && o.Status == OrderStatus.InCart);

            if (cart == null)
            {
                cart = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.UtcNow,
                    Status = OrderStatus.InCart,
                    TotalAmount = 0,
                    OrderItems = new List<OrderItem>()
                };
                _context.Orders.Add(cart);
            }

            // Check if book already in cart
            if (cart.OrderItems.Any(oi => oi.BookId == book.Id))
                throw new Exception("Book already in cart");

            var orderItem = new OrderItem
            {
                BookId = book.Id,
                PriceAtPurchase = book.Price,
                Order = cart
            };

            cart.OrderItems.Add(orderItem);
            cart.TotalAmount = cart.OrderItems.Sum(oi => oi.PriceAtPurchase);

            await _context.SaveChangesAsync();

            return await GetCartAsync(userId) ?? throw new Exception("Error getting cart");
        }

        public async Task<OrderDto> RemoveFromCartAsync(int userId, int orderItemId)
        {
            var cart = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.UserId == userId && o.Status == OrderStatus.InCart);

            if (cart == null)
                throw new Exception("Cart not found");

            var orderItem = cart.OrderItems.FirstOrDefault(oi => oi.Id == orderItemId);
            if (orderItem == null)
                throw new Exception("Item not found in cart");

            _context.OrderItems.Remove(orderItem);
            cart.TotalAmount = cart.OrderItems.Sum(oi => oi.PriceAtPurchase);

            await _context.SaveChangesAsync();

            return await GetCartAsync(userId) ?? throw new Exception("Error getting cart");
        }

        public async Task<OrderDto> CheckoutAsync(int userId, CheckoutDto checkoutDto)
        {
            var cart = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Book)
                .FirstOrDefaultAsync(o => o.UserId == userId && o.Status == OrderStatus.InCart);

            if (cart == null || !cart.OrderItems.Any())
                throw new Exception("Cart is empty");

            // If specific items are selected for payment
            if (checkoutDto.OrderItemIds.Any())
            {
                var itemsToRemove = cart.OrderItems
                    .Where(oi => !checkoutDto.OrderItemIds.Contains(oi.Id))
                    .ToList();

                foreach (var item in itemsToRemove)
                {
                    _context.OrderItems.Remove(item);
                }

                cart.OrderItems = cart.OrderItems
                    .Where(oi => checkoutDto.OrderItemIds.Contains(oi.Id))
                    .ToList();
            }

            // Update order status
            cart.Status = OrderStatus.Paid;
            cart.OrderDate = DateTime.UtcNow;
            cart.TotalAmount = cart.OrderItems.Sum(oi => oi.PriceAtPurchase);

            // Update reader statistics directly in User table
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                if (user.ReadBooksCount.HasValue)
                    user.ReadBooksCount += cart.OrderItems.Count;
                else
                    user.ReadBooksCount = cart.OrderItems.Count;
            }

            await _context.SaveChangesAsync();

            return MapToOrderDto(cart);
        }

        public async Task<IEnumerable<OrderDto>> GetUserOrdersAsync(int userId)
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Book)
                .Include(o => o.User)
                .Where(o => o.UserId == userId && o.Status == OrderStatus.Paid)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return orders.Select(MapToOrderDto);
        }

        public async Task<OrderDto?> GetOrderByIdAsync(int orderId, int userId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Book)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            return order != null ? MapToOrderDto(order) : null;
        }

        private OrderDto MapToOrderDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                UserId = order.UserId,
                UserName = order.User?.Nickname ?? "",
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status.ToString(),
                Items = order.OrderItems.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    BookId = oi.BookId,
                    BookTitle = oi.Book?.Title ?? "",
                    PriceAtPurchase = oi.PriceAtPurchase
                }).ToList()
            };
        }
    }
}