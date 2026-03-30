using BookPlatform.DTOs;
using BookPlatform.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookPlatform.Endpoints
{
    public class BadWordEndpoints : IEndpointDefinition
    {
        public void MapEndpoints(WebApplication app)
        {
            var group = app.MapGroup("/api/badwords")
                .WithTags("Bad Words")
                .RequireAuthorization();

            group.MapGet("/", GetAllBadWords)
                .WithName("GetAllBadWords");

            group.MapPost("/", AddBadWord)
                .WithName("AddBadWord");

            group.MapDelete("/{id:int}", DeleteBadWord)
                .WithName("DeleteBadWord");

            group.MapPost("/check-book/{bookId:int}", CheckBook)
                .WithName("CheckBookForBadWords");
        }

        private static async Task<IResult> GetAllBadWords(IBadWordService service)
        {
            var words = await service.GetAllBadWordsAsync();
            return Results.Ok(words);
        }

        private static async Task<IResult> AddBadWord(
            HttpContext httpContext,
            [FromBody] CreateBadWordDto createDto,
            IBadWordService service)
        {
            var adminId = GetUserId(httpContext);
            var word = await service.AddBadWordAsync(adminId, createDto);
            return Results.Ok(word);
        }

        private static async Task<IResult> DeleteBadWord(
            int id,
            HttpContext httpContext,
            IBadWordService service)
        {
            var adminId = GetUserId(httpContext);
            var result = await service.DeleteBadWordAsync(id, adminId);
            return result
                ? Results.Ok(new { message = "Deleted" })
                : Results.NotFound();
        }

        private static async Task<IResult> CheckBook(
            int bookId,
            HttpContext httpContext,
            IBadWordService service)
        {
            var editorId = GetUserId(httpContext);
            var result = await service.CheckBookForBadWordsAsync(bookId, editorId);
            return Results.Ok(result);
        }

        private static int GetUserId(HttpContext httpContext)
        {
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }
    }
}