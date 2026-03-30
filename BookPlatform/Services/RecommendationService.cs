using Microsoft.EntityFrameworkCore;
using BookPlatform.Data;
using BookPlatform.Models;
using BookPlatform.DTOs;

namespace BookPlatform.Services
{
    public class RecommendationService : IRecommendationService
    {
        private readonly ApplicationDbContext _context;

        public RecommendationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<RecommendationDto>> GetUserRecommendationsAsync(int userId)
        {
            var recommendations = await _context.Recommendations
                .Include(r => r.RecommendedBook)
                    .ThenInclude(b => b.Author)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.RelevanceScore)
                .Take(20)
                .ToListAsync();

            // Remove old recommendations (older than 7 days)
            var oldRecommendations = await _context.Recommendations
                .Where(r => r.UserId == userId && r.GeneratedDate < DateTime.UtcNow.AddDays(-7))
                .ToListAsync();

            if (oldRecommendations.Any())
            {
                _context.Recommendations.RemoveRange(oldRecommendations);
                await _context.SaveChangesAsync();
            }

            return recommendations.Select(r => new RecommendationDto
            {
                Id = r.Id,
                BookId = r.RecommendedBookId,
                BookTitle = r.RecommendedBook.Title,
                AuthorName = r.RecommendedBook.Author.Nickname,
                Rating = r.RecommendedBook.Rating,
                Price = r.RecommendedBook.Price,
                CoverImageUrl = r.RecommendedBook.CoverImageUrl ?? "",
                RelevanceScore = r.RelevanceScore,
                GeneratedDate = r.GeneratedDate
            });
        }

        public async Task<List<RecommendationDto>> GenerateRecommendationsAsync(int userId)
        //проверка пуста ли инфа об человеке, существует ли такой полбьзователь? 
        {
            var user = await _context.Users
                .Include(u => u.ReadingProgresses)
                    .ThenInclude(rp => rp.Book)
                .FirstOrDefaultAsync(u => u.Id == userId);

            // Get books that user has already read or purchased
            var readBookIds = await _context.ReadingProgresses
                .Where(rp => rp.UserId == userId)
                .Select(rp => rp.BookId)
                .ToListAsync();

            var purchasedBookIds = await _context.Orders
                .Where(o => o.UserId == userId && o.Status == OrderStatus.Paid)
                .SelectMany(o => o.OrderItems.Select(oi => oi.BookId))
                .ToListAsync();

            var excludedBookIds = readBookIds.Union(purchasedBookIds).Distinct().ToList();

            // Get popular books that user hasn't read yet
            var popularBooks = await _context.Books
                .Include(b => b.Author)
                .Where(b => b.Status == BookStatus.Published && !excludedBookIds.Contains(b.Id))
                .OrderByDescending(b => b.Rating)
                .ThenByDescending(b => b.ReadingProgresses.Count)
                .Take(50)
                .ToListAsync();

            // Remove old recommendations
            var oldRecommendations = await _context.Recommendations
                .Where(r => r.UserId == userId)
                .ToListAsync();

            _context.Recommendations.RemoveRange(oldRecommendations);
            await _context.SaveChangesAsync();

            // Create new recommendations
            var newRecommendations = new List<Recommendation>();
            var random = new Random();

            foreach (var book in popularBooks.Take(20))
            {
                // Calculate relevance based on rating and random factor
                var relevanceScore = book.Rating + (float)random.NextDouble() * 2;

                var recommendation = new Recommendation
                {
                    UserId = userId,
                    RecommendedBookId = book.Id,
                    GeneratedDate = DateTime.UtcNow,
                    RelevanceScore = relevanceScore
                };

                newRecommendations.Add(recommendation);
            }

            _context.Recommendations.AddRange(newRecommendations);
            await _context.SaveChangesAsync();

            return newRecommendations.Select(r => new RecommendationDto
            {
                Id = r.Id,
                BookId = r.RecommendedBookId,
                BookTitle = popularBooks.First(b => b.Id == r.RecommendedBookId).Title,
                AuthorName = popularBooks.First(b => b.Id == r.RecommendedBookId).Author.Nickname,
                Rating = popularBooks.First(b => b.Id == r.RecommendedBookId).Rating,
                Price = popularBooks.First(b => b.Id == r.RecommendedBookId).Price,
                CoverImageUrl = popularBooks.First(b => b.Id == r.RecommendedBookId).CoverImageUrl ?? "",
                RelevanceScore = r.RelevanceScore,
                GeneratedDate = r.GeneratedDate
            }).ToList();
        }

        public async Task<RecommendationDto?> GetRandomBookForReaderAsync(int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new Exception("User profile not found");

            // Get books that user has already read
            var readBookIds = await _context.ReadingProgresses
                .Where(rp => rp.UserId == userId)
                .Select(rp => rp.BookId)
                .ToListAsync();

            // Get available books
            var availableBooks = await _context.Books
                .Include(b => b.Author)
                .Where(b => b.Status == BookStatus.Published && !readBookIds.Contains(b.Id))
                .ToListAsync();

            if (!availableBooks.Any())
                return null;

            // Select random book
            var random = new Random();
            var randomBook = availableBooks[random.Next(availableBooks.Count)];

            // Update random book button in profile
            user.RandomBookButton = $"Last recommendation: {randomBook.Title}";
            await _context.SaveChangesAsync();

            return new RecommendationDto
            {
                BookId = randomBook.Id,
                BookTitle = randomBook.Title,
                AuthorName = randomBook.Author.Nickname,
                Rating = randomBook.Rating,
                Price = randomBook.Price,
                CoverImageUrl = randomBook.CoverImageUrl ?? "",
                GeneratedDate = DateTime.UtcNow
            };
        }
    }
}