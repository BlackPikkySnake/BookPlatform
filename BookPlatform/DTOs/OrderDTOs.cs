using System.ComponentModel.DataAnnotations;

namespace BookPlatform.DTOs
{
    public class OrderDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<OrderItemDto> Items { get; set; } = new();
    }

    public class OrderItemDto
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public decimal PriceAtPurchase { get; set; }
    }

    public class AddToCartDto
    {
        [Required]
        public int BookId { get; set; }
    }

    public class CheckoutDto
    {
        public List<int> OrderItemIds { get; set; } = new();
    }
}