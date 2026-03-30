using BookPlatform.DTOs;

namespace BookPlatform.Services
{
    public interface IBadWordService
    {
        Task<IEnumerable<BadWordDto>> GetAllBadWordsAsync();
        Task<BadWordDto> AddBadWordAsync(int adminId, CreateBadWordDto createBadWordDto);
        Task<bool> DeleteBadWordAsync(int id, int adminId);
        Task<BookCheckResultDto> CheckBookForBadWordsAsync(int bookId, int editorId);
    }
}