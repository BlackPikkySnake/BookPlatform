namespace BookPlatform.DTOs
{
    public class RecommendationDto
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public float Rating { get; set; }
        public decimal Price { get; set; }
        public string CoverImageUrl { get; set; } = string.Empty;
        public float RelevanceScore { get; set; }
        public DateTime GeneratedDate { get; set; }
    }
}