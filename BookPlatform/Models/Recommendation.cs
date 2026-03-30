using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookPlatform.Models
{
    public class Recommendation
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        [ForeignKey("RecommendedBook")]
        public int RecommendedBookId { get; set; }

        public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;

        public float RelevanceScore { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual Book RecommendedBook { get; set; } = null!;
    }
}