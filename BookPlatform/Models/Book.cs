using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookPlatform.Models
{
    public class Book
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        public DateTime DateWritten { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public float Rating { get; set; }

        [ForeignKey("Author")]
        public int AuthorUserId { get; set; }

        [ForeignKey("Editor")]
        public int? EditorId { get; set; }

        public BookStatus Status { get; set; } = BookStatus.Draft;

        public string? Content { get; set; }

        public int PageCount { get; set; }

        public string? CoverImageUrl { get; set; }

        // Navigation properties
        public virtual User Author { get; set; } = null!;
        public virtual User? Editor { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual ICollection<ReadingProgress> ReadingProgresses { get; set; } = new List<ReadingProgress>();
        public virtual ICollection<Recommendation> Recommendations { get; set; } = new List<Recommendation>();
    }
}