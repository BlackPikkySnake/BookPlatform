namespace BookPlatform.Models
{
    // Book status
    public enum BookStatus
    {
        Draft = 1,       // Work in progress
        UnderReview = 2, // Waiting for editor check
        Published = 3,   // Available for purchase
        Rejected = 4     // Failed moderation
    }
}