using System.ComponentModel.DataAnnotations;

namespace BookPlatform.DTOs
{
    public class BookDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime DateWritten { get; set; }
        public decimal Price { get; set; }
        public float Rating { get; set; }
        public int AuthorUserId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public int? EditorId { get; set; }
        public string Status { get; set; } = string.Empty;
        public int PageCount { get; set; }
        public string? CoverImageUrl { get; set; }
    }

    public class CreateBookDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        public DateTime DateWritten { get; set; } = DateTime.UtcNow;

        [Range(0, 999999)]
        public decimal Price { get; set; }

        public string? Content { get; set; }

        [Range(1, 10000)]
        public int PageCount { get; set; }

        public string? CoverImageUrl { get; set; }
    }

    public class UpdateBookDto
    {
        public string? Title { get; set; }
        public decimal? Price { get; set; }
        public string? Content { get; set; }
        public int? PageCount { get; set; }
        public string? CoverImageUrl { get; set; }
        public string? Status { get; set; }
    }

    public class ReadingProgressDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public DateTime LastReadDate { get; set; }
        public int LastPage { get; set; }
        public bool IsCompleted { get; set; }
    }

    public class UpdateReadingProgressDto
    {
        [Range(1, 10000)]
        public int LastPage { get; set; }
        public bool IsCompleted { get; set; }
    }

    public class BookDetailsDto : BookDto
    {
        public string? Content { get; set; }
        public List<ReadingProgressDto>? ReadingProgresses { get; set; }
    }
}