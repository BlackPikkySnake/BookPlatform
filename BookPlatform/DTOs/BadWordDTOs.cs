using System.ComponentModel.DataAnnotations;

namespace BookPlatform.DTOs
{
    public class BadWordDto
    {
        public int Id { get; set; }
        public string Word { get; set; } = string.Empty;
        public int AddedByAdminId { get; set; }
        public string AddedByAdminName { get; set; } = string.Empty;
        public DateTime AddedDate { get; set; }
    }

    public class CreateBadWordDto
    {
        [Required]
        [StringLength(100)]
        public string Word { get; set; } = string.Empty;
    }

    public class BookCheckResultDto
    {
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public bool HasBadWords { get; set; }
        public List<string> FoundBadWords { get; set; } = new();
        public string NewStatus { get; set; } = string.Empty;
    }
}