using Microsoft.EntityFrameworkCore;
using BookPlatform.Data;
using BookPlatform.Models;
using BookPlatform.DTOs;

namespace BookPlatform.Services
{
    public class BookService : IBookService
    {
        private readonly ApplicationDbContext _context;

        public BookService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BookDto>> GetAllBooksAsync(string? status = null, string? author = null)
        {
            var query = _context.Books
                .Include(b => b.Author)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<BookStatus>(status, true, out var bookStatus))
            {
                query = query.Where(b => b.Status == bookStatus);
            }

            if (!string.IsNullOrEmpty(author))
            {
                query = query.Where(b => b.Author.Nickname.Contains(author) || b.Author.FullName.Contains(author));
            }

            var books = await query.ToListAsync();

            return books.Select(b => new BookDto
            {
                Id = b.Id,
                Title = b.Title,
                DateWritten = b.DateWritten,
                Price = b.Price,
                Rating = b.Rating,
                AuthorUserId = b.AuthorUserId,
                AuthorName = b.Author.Nickname,
                EditorId = b.EditorId,
                Status = b.Status.ToString(),
                PageCount = b.PageCount,
                CoverImageUrl = b.CoverImageUrl
            });
        }

        public async Task<BookDetailsDto?> GetBookByIdAsync(int id)
        {
            var book = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.ReadingProgresses)
                    .ThenInclude(rp => rp.User)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
                return null;

            return new BookDetailsDto
            {
                Id = book.Id,
                Title = book.Title,
                DateWritten = book.DateWritten,
                Price = book.Price,
                Rating = book.Rating,
                AuthorUserId = book.AuthorUserId,
                AuthorName = book.Author.Nickname,
                EditorId = book.EditorId,
                Status = book.Status.ToString(),
                PageCount = book.PageCount,
                CoverImageUrl = book.CoverImageUrl,
                Content = book.Content,
                ReadingProgresses = book.ReadingProgresses.Select(rp => new ReadingProgressDto
                {
                    UserId = rp.UserId,
                    UserName = rp.User.Nickname,
                    BookId = rp.BookId,
                    LastReadDate = rp.LastReadDate,
                    LastPage = rp.LastPage,
                    IsCompleted = rp.IsCompleted
                }).ToList()
            };
        }

        public async Task<BookDto> CreateBookAsync(int authorId, CreateBookDto createBookDto)
        {
            var author = await _context.Users.FirstOrDefaultAsync(u => u.Id == authorId);
            //добавить проверку на созданн ли автор? Естть ли такой в базе?
            var book = new Book
            {
                Title = createBookDto.Title,
                DateWritten = createBookDto.DateWritten,
                Price = createBookDto.Price,
                Content = createBookDto.Content,
                PageCount = createBookDto.PageCount,
                CoverImageUrl = createBookDto.CoverImageUrl,
                AuthorUserId = authorId,
                Status = BookStatus.Draft,
                Rating = 0
            };

            _context.Books.Add(book);

            // Update author's in-progress count
            if (author.InProgressBooksCount.HasValue)
                author.InProgressBooksCount++;
            else
                author.InProgressBooksCount = 1;

            await _context.SaveChangesAsync();

            return new BookDto
            {
                Id = book.Id,
                Title = book.Title,
                DateWritten = book.DateWritten,
                Price = book.Price,
                Rating = book.Rating,
                AuthorUserId = book.AuthorUserId,
                AuthorName = author.Nickname,
                Status = book.Status.ToString(),
                PageCount = book.PageCount,
                CoverImageUrl = book.CoverImageUrl
            };
        }

        public async Task<BookDto?> UpdateBookAsync(int id, int userId, UpdateBookDto updateBookDto)
        {
            var book = await _context.Books
                .Include(b => b.Author)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
                return null;

            // Check permissions: author or editor/admin
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return null;

            if (book.AuthorUserId != userId && user.Role != UserRole.Editor && user.Role != UserRole.Admin)
            {
                throw new Exception("Don't have the rights to edit this book.");
            }

            if (updateBookDto.Title != null)
                book.Title = updateBookDto.Title;

            if (updateBookDto.Price.HasValue)
                book.Price = updateBookDto.Price.Value;

            if (updateBookDto.Content != null)
                book.Content = updateBookDto.Content;

            if (updateBookDto.PageCount.HasValue)
                book.PageCount = updateBookDto.PageCount.Value;

            if (updateBookDto.CoverImageUrl != null)
                book.CoverImageUrl = updateBookDto.CoverImageUrl;

            if (updateBookDto.Status != null && Enum.TryParse<BookStatus>(updateBookDto.Status, true, out var newStatus))
            {
                book.Status = newStatus;

                // If status changes to UnderReview, reset editor
                if (newStatus == BookStatus.UnderReview)
                {
                    book.EditorId = null;
                }
            }

            await _context.SaveChangesAsync();

            return new BookDto
            {
                Id = book.Id,
                Title = book.Title,
                DateWritten = book.DateWritten,
                Price = book.Price,
                Rating = book.Rating,
                AuthorUserId = book.AuthorUserId,
                AuthorName = book.Author.Nickname,
                EditorId = book.EditorId,
                Status = book.Status.ToString(),
                PageCount = book.PageCount,
                CoverImageUrl = book.CoverImageUrl
            };
        }

        public async Task<bool> DeleteBookAsync(int id, int userId)
        {
            var book = await _context.Books
                .Include(b => b.Author)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
                return false;

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            if (book.AuthorUserId != userId && user.Role != UserRole.Admin)
            {
                throw new Exception("Only author or admin can delete a book");
            }

            // Update author's counts
            if (book.Status == BookStatus.Draft && book.Author.InProgressBooksCount.HasValue && book.Author.InProgressBooksCount > 0)
            {
                book.Author.InProgressBooksCount--;
            }
            else if (book.Status == BookStatus.Published && book.Author.PublishedBooksCount.HasValue && book.Author.PublishedBooksCount > 0)
            {
                book.Author.PublishedBooksCount--;
            }

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<BookDto?> PublishBookAsync(int id, int editorId)
        {
            var book = await _context.Books
                .Include(b => b.Author)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
                return null;

            var editor = await _context.Users.FindAsync(editorId);
            if (editor == null || (editor.Role != UserRole.Editor && editor.Role != UserRole.Admin))
            {
                throw new Exception("Only editor or admin can publish books");
            }

            if (book.Status != BookStatus.UnderReview)
            {
                throw new Exception("Book must be under review before publishing");
            }

            book.Status = BookStatus.Published;
            book.EditorId = editorId;
            book.Rating = 5; // Initial rating

            // Update author's counts
            if (book.Author.InProgressBooksCount.HasValue && book.Author.InProgressBooksCount > 0)
            {
                book.Author.InProgressBooksCount--;
            }

            if (book.Author.PublishedBooksCount.HasValue)
                book.Author.PublishedBooksCount++;
            else
                book.Author.PublishedBooksCount = 1;

            await _context.SaveChangesAsync();

            return new BookDto
            {
                Id = book.Id,
                Title = book.Title,
                DateWritten = book.DateWritten,
                Price = book.Price,
                Rating = book.Rating,
                AuthorUserId = book.AuthorUserId,
                AuthorName = book.Author.Nickname,
                EditorId = book.EditorId,
                Status = book.Status.ToString(),
                PageCount = book.PageCount,
                CoverImageUrl = book.CoverImageUrl
            };
        }

        public async Task<ReadingProgressDto> UpdateReadingProgressAsync(int userId, int bookId, UpdateReadingProgressDto progressDto)
        {
            var progress = await _context.ReadingProgresses
                .Include(rp => rp.User)
                .Include(rp => rp.Book)
                .FirstOrDefaultAsync(rp => rp.UserId == userId && rp.BookId == bookId);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (progress == null)
            {
                progress = new ReadingProgress
                {
                    UserId = userId,
                    BookId = bookId,
                    LastPage = progressDto.LastPage,
                    LastReadDate = DateTime.UtcNow,
                    IsCompleted = progressDto.IsCompleted
                };
                _context.ReadingProgresses.Add(progress);

                // Update reader's reading books count
                if (user != null)
                {
                    if (user.ReadingBooksCount.HasValue)
                        user.ReadingBooksCount++;
                    else
                        user.ReadingBooksCount = 1;
                }
            }
            else
            {
                // If book was completed and now not completed
                if (progress.IsCompleted && !progressDto.IsCompleted)
                {
                    if (user != null)
                    {
                        if (user.ReadBooksCount.HasValue && user.ReadBooksCount > 0)
                            user.ReadBooksCount--;
                        if (user.ReadingBooksCount.HasValue)
                            user.ReadingBooksCount++;
                        else
                            user.ReadingBooksCount = 1;
                    }
                }
                // If book is now completed
                else if (!progress.IsCompleted && progressDto.IsCompleted)
                {
                    if (user != null)
                    {
                        if (user.ReadBooksCount.HasValue)
                            user.ReadBooksCount++;
                        else
                            user.ReadBooksCount = 1;

                        if (user.ReadingBooksCount.HasValue && user.ReadingBooksCount > 0)
                            user.ReadingBooksCount--;
                    }
                }

                progress.LastPage = progressDto.LastPage;
                progress.LastReadDate = DateTime.UtcNow;
                progress.IsCompleted = progressDto.IsCompleted;
            }

            await _context.SaveChangesAsync();

            return new ReadingProgressDto
            {
                UserId = progress.UserId,
                UserName = user?.Nickname ?? "",
                BookId = progress.BookId,
                BookTitle = progress.Book?.Title ?? "",
                LastReadDate = progress.LastReadDate,
                LastPage = progress.LastPage,
                IsCompleted = progress.IsCompleted
            };
        }

        public async Task<IEnumerable<ReadingProgressDto>> GetUserReadingProgressAsync(int userId)
        {
            var progresses = await _context.ReadingProgresses
                .Include(rp => rp.Book)
                .Where(rp => rp.UserId == userId)
                .ToListAsync();

            return progresses.Select(rp => new ReadingProgressDto
            {
                UserId = rp.UserId,
                BookId = rp.BookId,
                BookTitle = rp.Book?.Title ?? "",
                LastReadDate = rp.LastReadDate,
                LastPage = rp.LastPage,
                IsCompleted = rp.IsCompleted
            });
        }
    }
}