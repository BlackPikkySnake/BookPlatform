using BookPlatform.DTOs;

namespace BookPlatform.Services
{
    public interface IBookService
    {
        Task<IEnumerable<BookDto>> GetAllBooksAsync(string? status = null, string? author = null);
        Task<BookDetailsDto?> GetBookByIdAsync(int id);
        Task<BookDto> CreateBookAsync(int authorId, CreateBookDto createBookDto);
        Task<BookDto?> UpdateBookAsync(int id, int userId, UpdateBookDto updateBookDto);
        Task<bool> DeleteBookAsync(int id, int userId);
        Task<BookDto?> PublishBookAsync(int id, int editorId);
        Task<ReadingProgressDto> UpdateReadingProgressAsync(int userId, int bookId, UpdateReadingProgressDto progressDto);
        Task<IEnumerable<ReadingProgressDto>> GetUserReadingProgressAsync(int userId);
    }
}