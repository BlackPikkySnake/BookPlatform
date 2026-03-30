namespace BookPlatform.Models
{
    // User roles - all in English
    public enum UserRole
    {
        User = 1,      // Can write books
        Editor = 2,      // Can review and edit books
        Admin = 3        // Full system access
    }
}