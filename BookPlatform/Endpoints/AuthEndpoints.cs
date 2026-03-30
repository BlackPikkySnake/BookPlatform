using BookPlatform.DTOs;
using BookPlatform.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookPlatform.Endpoints
{
    public class AuthEndpoints : IEndpointDefinition
    {
        public void MapEndpoints(WebApplication app)
        {
            // Группа всех эндпоинтов авторизации
            var group = app.MapGroup("/api/auth")
                .WithTags("Authentication");

            // POST /api/auth/register - регистрация
            group.MapPost("/register", RegisterUser)
                .WithName("RegisterUser")
                .WithDescription("Register a new user");

            // POST /api/auth/login - вход
            group.MapPost("/login", LoginUser)
                .WithName("LoginUser")
                .WithDescription("Login with email and password");
        }

        private static async Task<IResult> RegisterUser(
            [FromBody] RegisterDto registerDto,
            IAuthService authService)
        {
            try
            {
                var result = await authService.RegisterAsync(registerDto);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        }

        private static async Task<IResult> LoginUser(
            [FromBody] LoginDto loginDto,
            IAuthService authService)
        {
            try
            {
                var result = await authService.LoginAsync(loginDto);
                return Results.Ok(result);
            }
            catch (Exception)
            {
                return Results.Unauthorized();
            }
        }
    }
}