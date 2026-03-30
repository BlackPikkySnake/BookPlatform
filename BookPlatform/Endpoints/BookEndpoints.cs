using BookPlatform.DTOs;
using BookPlatform.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookPlatform.Endpoints
{
    public class BookEndpoints : IEndpointDefinition
    {
        public void MapEndpoints(WebApplication app)
        {
            // Группа эндпоинтов книг
            var group = app.MapGroup("/api/books")
                .WithTags("Books");

            // Публичные эндпоинты (без авторизации)
            group.MapGet("/", GetAllBooks)
                .WithName("GetAllBooks")
                .WithDescription("Get all books");

            group.MapGet("/{id:int}", GetBookById)
                .WithName("GetBookById")
                .WithDescription("Get book by ID");

            // Защищенные эндпоинты (требуют авторизацию)
            group.MapPost("/", CreateBook)
                .WithName("CreateBook")
                .RequireAuthorization();

            group.MapPut("/{id:int}", UpdateBook)
                .WithName("UpdateBook")
                .RequireAuthorization();

            group.MapDelete("/{id:int}", DeleteBook)
                .WithName("DeleteBook")
                .RequireAuthorization();

            // Прогресс чтения
            group.MapPost("/{bookId:int}/progress", UpdateProgress)
                .WithName("UpdateProgress")
                .RequireAuthorization();

            group.MapGet("/progress", GetMyProgress)
                .WithName("GetMyProgress")
                .RequireAuthorization();
        }

        private static async Task<IResult> GetAllBooks(
            [FromQuery] string? status,
            [FromQuery] string? author,
            IBookService bookService)
        {
            var books = await bookService.GetAllBooksAsync(status, author);
            return Results.Ok(books);
        }

        private static async Task<IResult> GetBookById(
            int id,
            IBookService bookService)
        {
            var book = await bookService.GetBookByIdAsync(id);
            return book == null
                ? Results.NotFound(new { message = "Book not found" })
                : Results.Ok(book);
        }

        private static async Task<IResult> CreateBook(
            HttpContext httpContext,
            [FromBody] CreateBookDto createDto,
            IBookService bookService)
        {
            try
            {
                var userId = GetUserId(httpContext);
                var book = await bookService.CreateBookAsync(userId, createDto);
                return Results.Created($"/api/books/{book.Id}", book);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        }

        private static async Task<IResult> UpdateBook(
            int id,
            HttpContext httpContext,
            [FromBody] UpdateBookDto updateDto,
            IBookService bookService)
        {
            try
            {
                var userId = GetUserId(httpContext);
                var book = await bookService.UpdateBookAsync(id, userId, updateDto);
                return book == null
                    ? Results.NotFound(new { message = "Book not found" })
                    : Results.Ok(book);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        }

        private static async Task<IResult> DeleteBook(
            int id,
            HttpContext httpContext,
            IBookService bookService)
        {
            try
            {
                var userId = GetUserId(httpContext);
                var result = await bookService.DeleteBookAsync(id, userId);
                return result
                    ? Results.Ok(new { message = "Book deleted" })
                    : Results.NotFound(new { message = "Book not found" });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        }

        private static async Task<IResult> UpdateProgress(
            int bookId,
            HttpContext httpContext,
            [FromBody] UpdateReadingProgressDto progressDto,
            IBookService bookService)
        {
            var userId = GetUserId(httpContext);
            var progress = await bookService.UpdateReadingProgressAsync(userId, bookId, progressDto);
            return Results.Ok(progress);
        }

        private static async Task<IResult> GetMyProgress(
            HttpContext httpContext,
            IBookService bookService)
        {
            var userId = GetUserId(httpContext);
            var progress = await bookService.GetUserReadingProgressAsync(userId);
            return Results.Ok(progress);
        }

        private static int GetUserId(HttpContext httpContext)
        {
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }
    }
}