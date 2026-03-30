namespace BookPlatform.Models
{
    // Order status
    public enum OrderStatus
    {
        InCart = 1,      // Not paid yet
        Paid = 2,        // Payment completed
        Cancelled = 3    // Order cancelled
    }
}