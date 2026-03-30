using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookPlatform.Models
{
    public class BadWord
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Word { get; set; } = string.Empty;

        [ForeignKey("AddedByAdmin")]
        public int AddedByAdminId { get; set; }

        public DateTime AddedDate { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual User AddedByAdmin { get; set; } = null!;
    }
}