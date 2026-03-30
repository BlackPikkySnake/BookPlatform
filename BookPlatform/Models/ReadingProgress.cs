using System.ComponentModel.DataAnnotations.Schema;

namespace BookPlatform.Models
{
    public class ReadingProgress
    {
        [ForeignKey("User")]
        public int UserId { get; set; }

        [ForeignKey("Book")]
        public int BookId { get; set; }

        public DateTime LastReadDate { get; set; } = DateTime.UtcNow;

        public int LastPage { get; set; }

        public bool IsCompleted { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual Book Book { get; set; } = null!;
    }
}