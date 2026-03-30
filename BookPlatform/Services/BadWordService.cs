using Microsoft.EntityFrameworkCore;
using BookPlatform.Data;
using BookPlatform.Models;
using BookPlatform.DTOs;
using System.Text.RegularExpressions;

namespace BookPlatform.Services
{
    public class BadWordService : IBadWordService
    {
        private readonly ApplicationDbContext _context;

        public BadWordService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BadWordDto>> GetAllBadWordsAsync()
        {
            var badWords = await _context.BadWords
                .Include(bw => bw.AddedByAdmin)
                .OrderBy(bw => bw.Word)
                .ToListAsync();

            return badWords.Select(bw => new BadWordDto
            {
                Id = bw.Id,
                Word = bw.Word,
                AddedByAdminId = bw.AddedByAdminId,
                AddedByAdminName = bw.AddedByAdmin?.Nickname ?? "",
                AddedDate = bw.AddedDate
            });
        }

        public async Task<BadWordDto> AddBadWordAsync(int adminId, CreateBadWordDto createBadWordDto)
        {
            var admin = await _context.Users.FindAsync(adminId);
            if (admin == null || (admin.Role != UserRole.Admin && admin.Role != UserRole.Editor))
            {
                throw new Exception("Только администратор или редактор может добавлять плохие слова");
            }

            // Проверяем, не существует ли уже такое слово
            var existingWord = await _context.BadWords
                .FirstOrDefaultAsync(bw => bw.Word.ToLower() == createBadWordDto.Word.ToLower());

            if (existingWord != null)
            {
                throw new Exception("Такое слово уже существует в списке");
            }

            var badWord = new BadWord
            {
                Word = createBadWordDto.Word.Trim(),
                AddedByAdminId = adminId,
                AddedDate = DateTime.UtcNow
            };

            _context.BadWords.Add(badWord);
            await _context.SaveChangesAsync();

            return new BadWordDto
            {
                Id = badWord.Id,
                Word = badWord.Word,
                AddedByAdminId = badWord.AddedByAdminId,
                AddedByAdminName = admin.Nickname,
                AddedDate = badWord.AddedDate
            };
        }

        public async Task<bool> DeleteBadWordAsync(int id, int adminId)
        {
            var admin = await _context.Users.FindAsync(adminId);
            if (admin == null || admin.Role != UserRole.Admin)
            {
                throw new Exception("Только администратор может удалять плохие слова");
            }

            var badWord = await _context.BadWords.FindAsync(id);
            if (badWord == null)
                return false;

            _context.BadWords.Remove(badWord);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<BookCheckResultDto> CheckBookForBadWordsAsync(int bookId, int editorId)
        {
            var book = await _context.Books
                .Include(b => b.Author)
                .FirstOrDefaultAsync(b => b.Id == bookId);

            if (book == null)
                throw new Exception("Книга не найдена");

            var editor = await _context.Users.FindAsync(editorId);
            if (editor == null || (editor.Role != UserRole.Editor && editor.Role != UserRole.Admin))
            {
                throw new Exception("Только редактор или администратор может проверять книги");
            }

            if (string.IsNullOrEmpty(book.Content))
            {
                throw new Exception("Книга не содержит текста для проверки");
            }

            var badWords = await _context.BadWords.ToListAsync();
            var foundBadWords = new List<string>();

            // Проверяем наличие плохих слов в тексте
            foreach (var badWord in badWords)
            {
                var pattern = $@"\b{Regex.Escape(badWord.Word)}\b";
                if (Regex.IsMatch(book.Content, pattern, RegexOptions.IgnoreCase))
                {
                    foundBadWords.Add(badWord.Word);
                }
            }

            var result = new BookCheckResultDto
            {
                BookId = book.Id,
                BookTitle = book.Title,
                HasBadWords = foundBadWords.Any(),
                FoundBadWords = foundBadWords
            };

            // Обновляем статус книги
            if (foundBadWords.Any())
            {
                book.Status = BookStatus.Rejected;
                result.NewStatus = BookStatus.Rejected.ToString();
            }
            else
            {
                book.Status = BookStatus.UnderReview;
                result.NewStatus = BookStatus.UnderReview.ToString();
            }

            book.EditorId = editorId;
            await _context.SaveChangesAsync();

            return result;
        }
    }
}