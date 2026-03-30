using BookPlatform.Data;
using BookPlatform.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookPlatform.Endpoints
{
    public class UserEndpoints : IEndpointDefinition
    {
        public void MapEndpoints(WebApplication app)
        {
            // Группа эндпоинтов пользователей (требуют авторизацию)
            var group = app.MapGroup("/api/users")
                .WithTags("Users")
                .RequireAuthorization();

            // GET /api/users/me - получить текущего пользователя
            group.MapGet("/me", GetCurrentUser)
                .WithName("GetCurrentUser");

            // GET /api/users/{id} - получить пользователя по ID
            group.MapGet("/{id:int}", GetUserById)
                .WithName("GetUserById");

            // PUT /api/users/me - обновить текущего пользователя
            group.MapPut("/me", UpdateCurrentUser)
                .WithName("UpdateCurrentUser");
        }

        private static async Task<IResult> GetCurrentUser(
            HttpContext httpContext,
            ApplicationDbContext dbContext)
        {
            var userId = GetUserId(httpContext);
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return Results.NotFound();

            return Results.Ok(new UserDto
            {
                Id = user.Id,
                Nickname = user.Nickname,
                FullName = user.FullName,
                Email = user.Email,
                RegistrationDate = user.RegistrationDate,
                Level = user.Level,
                Role = user.Role.ToString(),
                PublishedBooksCount = user.PublishedBooksCount,
                InProgressBooksCount = user.InProgressBooksCount,
                Biography = user.Biography,
                PenName = user.PenName,
                ReadBooksCount = user.ReadBooksCount,
                FavoriteGenres = user.FavoriteGenres
            });
        }

        private static async Task<IResult> GetUserById(
            int id,
            ApplicationDbContext dbContext)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
                return Results.NotFound();

            return Results.Ok(new UserDto
            {
                Id = user.Id,
                Nickname = user.Nickname,
                FullName = user.FullName,
                Email = user.Email,
                RegistrationDate = user.RegistrationDate,
                Level = user.Level,
                Role = user.Role.ToString()
            });
        }

        private static async Task<IResult> UpdateCurrentUser(
            HttpContext httpContext,
            [FromBody] UpdateUserDto updateDto,
            ApplicationDbContext dbContext)
        {
            var userId = GetUserId(httpContext);
            var user = await dbContext.Users.FindAsync(userId);

            if (user == null)
                return Results.NotFound();

            if (!string.IsNullOrEmpty(updateDto.FullName))
                user.FullName = updateDto.FullName;

            if (!string.IsNullOrEmpty(updateDto.Nickname))
                user.Nickname = updateDto.Nickname;

            if (updateDto.Biography != null)
                user.Biography = updateDto.Biography;

            if (updateDto.PenName != null)
                user.PenName = updateDto.PenName;

            if (updateDto.FavoriteGenres != null)
                user.FavoriteGenres = updateDto.FavoriteGenres;

            await dbContext.SaveChangesAsync();
            return Results.Ok(new { message = "Profile updated" });
        }

        private static int GetUserId(HttpContext httpContext)
        {
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }
    }
}