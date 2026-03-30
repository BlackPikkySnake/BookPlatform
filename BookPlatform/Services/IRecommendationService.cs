using BookPlatform.DTOs;

namespace BookPlatform.Services
{
    public interface IRecommendationService
    {
        Task<IEnumerable<RecommendationDto>> GetUserRecommendationsAsync(int userId);
        Task<List<RecommendationDto>> GenerateRecommendationsAsync(int userId);
        Task<RecommendationDto?> GetRandomBookForReaderAsync(int userId);
    }
}