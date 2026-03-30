namespace BookPlatform.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Nickname { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; }
        public float Level { get; set; }
        public string Role { get; set; } = string.Empty;

        // Author fields
        public int? PublishedBooksCount { get; set; }
        public int? InProgressBooksCount { get; set; }
        public string? Biography { get; set; }
        public string? PenName { get; set; }

        // Reader fields
        public int? ReadingBooksCount { get; set; }
        public int? ReadBooksCount { get; set; }
        public string? FavoriteGenres { get; set; }
    }

    public class UpdateUserDto
    {
        public string? FullName { get; set; }
        public string? Nickname { get; set; }
        public string? Biography { get; set; }
        public string? PenName { get; set; }
        public string? FavoriteGenres { get; set; }
    }
}