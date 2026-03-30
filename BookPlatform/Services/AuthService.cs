using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BookPlatform.Data;
using BookPlatform.Models;
using BookPlatform.DTOs;
using BCrypt.Net;

namespace BookPlatform.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            // Check if user with this email already exists
            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
            {
                throw new Exception("User with this email already exists");
            }

            // Check if user with this nickname already exists
            if (await _context.Users.AnyAsync(u => u.Nickname == registerDto.Nickname))
            {
                throw new Exception("User with this nickname already exists");
            }

            // Parse role from string to enum
            if (!Enum.TryParse<UserRole>(registerDto.Role, true, out var userRole))
            {
                userRole = UserRole.User; // Default role
            }

            // Create new user
            var user = new User
            {
                Nickname = registerDto.Nickname,
                FullName = registerDto.FullName,
                Email = registerDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                RegistrationDate = DateTime.UtcNow,
                Role = userRole,
                Level = 1.0f
            };

            // Initialize author fields if role is Author or Admin
            if (user.Role == UserRole.User || user.Role == UserRole.Admin)
            {
                user.PublishedBooksCount = 0;
                user.InProgressBooksCount = 0;
                user.Biography = null;
                user.PenName = null;
                user.ReadingBooksCount = 0;
                user.ReadBooksCount = 0;
                user.FavoriteGenres = null;
                user.RandomBookButton = null;
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Generate JWT token
            var token = GenerateJwtToken(user);

            return new AuthResponseDto
            {
                Id = user.Id,
                Nickname = user.Nickname,
                Email = user.Email,
                Role = user.Role.ToString(),
                Level = user.Level,
                Token = token
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                throw new Exception("Invalid email or password");
            }

            var token = GenerateJwtToken(user);

            return new AuthResponseDto
            {
                Id = user.Id,
                Nickname = user.Nickname,
                Email = user.Email,
                Role = user.Role.ToString(),
                Level = user.Level,
                Token = token
            };
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? "your-secret-key-here-minimum-32-characters-long");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}