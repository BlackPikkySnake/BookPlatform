using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BookPlatform.Services;

namespace BookPlatform.Endpoints
{
    public class RecommendationEndpoints : IEndpointDefinition
    {
        public void MapEndpoints(WebApplication app)
        {
            var group = app.MapGroup("/api/recommendations")
                .WithTags("Recommendations")
                .RequireAuthorization();

            group.MapGet("/", GetRecommendations)
                .WithName("GetRecommendations");

            group.MapPost("/generate", GenerateRecommendations)
                .WithName("GenerateRecommendations");

            group.MapGet("/random-book", GetRandomBook)
                .WithName("GetRandomBook");
        }

        private static async Task<IResult> GetRecommendations(
            HttpContext httpContext,
            IRecommendationService service)
        {
            var userId = GetUserId(httpContext);
            var recs = await service.GetUserRecommendationsAsync(userId);

            if (!recs.Any())
                recs = await service.GenerateRecommendationsAsync(userId);

            return Results.Ok(recs);
        }

        private static async Task<IResult> GenerateRecommendations(
            HttpContext httpContext,
            IRecommendationService service)
        {
            var userId = GetUserId(httpContext);
            var recs = await service.GenerateRecommendationsAsync(userId);
            return Results.Ok(recs);
        }

        private static async Task<IResult> GetRandomBook(
            HttpContext httpContext,
            IRecommendationService service)
        {
            var userId = GetUserId(httpContext);
            var book = await service.GetRandomBookForReaderAsync(userId);
            return book == null
                ? Results.NotFound(new { message = "No books available" })
                : Results.Ok(book);
        }

        private static int GetUserId(HttpContext httpContext)
        {
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }
    }
}