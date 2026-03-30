using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookPlatform.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Nickname { get; set; } = string.Empty;

        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        public float Level { get; set; } = 1.0f;

        public UserRole Role { get; set; }

        // Author specific fields
        public int? PublishedBooksCount { get; set; }
        public int? InProgressBooksCount { get; set; }
        public string? Biography { get; set; }
        public string? PenName { get; set; }

        // Reader specific fields
        public int? ReadingBooksCount { get; set; }
        public int? ReadBooksCount { get; set; }
        public string? FavoriteGenres { get; set; }
        public string? RandomBookButton { get; set; }

        // Navigation properties
        public virtual ICollection<Book> WrittenBooks { get; set; } = new List<Book>();
        public virtual ICollection<Book> EditedBooks { get; set; } = new List<Book>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<ReadingProgress> ReadingProgresses { get; set; } = new List<ReadingProgress>();
        public virtual ICollection<Recommendation> Recommendations { get; set; } = new List<Recommendation>();
        public virtual ICollection<BadWord> AddedBadWords { get; set; } = new List<BadWord>();
    }
}