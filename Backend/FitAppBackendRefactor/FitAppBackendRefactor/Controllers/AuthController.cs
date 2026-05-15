using DiplomaFit.Data;
using DiplomaFit.Model.Dto.Auth;
using DiplomaFit.Model.Entities;
using DiplomaFit.Model.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace FitAppBackend.Api.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext ctx;
        private readonly IConfiguration config;
        private readonly PasswordHasher<User> passwordHasher;

        public AuthController(AppDbContext ctx, IConfiguration config)
        {
            this.ctx = ctx;
            this.config = config;
            passwordHasher = new PasswordHasher<User>();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var email = dto.Email.Trim().ToLower();

            var emailAlreadyExists = await ctx.Users
                .AnyAsync(x => x.Email.ToLower() == email);

            if (emailAlreadyExists)
            {
                return Conflict("Ezzel az email címmel már létezik felhasználó.");
            }

            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                Name = dto.Name.Trim(),
                Email = email,
                Gender = dto.Gender,
                Age = dto.Age,

                HeightCm = 0,
                WeightKg = 0,
                BodyfatPercent = null,
                ActivityLevel = 0,
                GoalType = 0,
                GoalDeltaKg = 0,
                GoalTimeWeeks = 0,

                ProfilePictureUrl = string.Empty
            };

            user.PasswordHash = passwordHasher.HashPassword(user, dto.Password);

            var refreshToken = GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

            ctx.Users.Add(user);
            await ctx.SaveChangesAsync();

            return Ok(CreateAuthResponse(user));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var email = dto.Email.Trim().ToLower();

            var user = await ctx.Users
                .SingleOrDefaultAsync(x => x.Email.ToLower() == email);

            if (user == null)
            {
                return BadRequest("Nincs ilyen felhasználó.");
            }

            if (string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                return BadRequest("Ehhez a felhasználóhoz nincs jelszó beállítva.");
            }

            var passwordResult = passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                dto.Password
            );

            if (passwordResult == PasswordVerificationResult.Failed)
            {
                return BadRequest("Hibás jelszó.");
            }

            var refreshToken = GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

            await ctx.SaveChangesAsync();

            return Ok(CreateAuthResponse(user));
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(RefreshTokenRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await ctx.Users
                .SingleOrDefaultAsync(x => x.Id == dto.UserId);

            if (user == null)
            {
                return Unauthorized("Érvénytelen felhasználó.");
            }

            if (string.IsNullOrWhiteSpace(user.RefreshToken))
            {
                return Unauthorized("Nincs refresh token.");
            }

            if (user.RefreshToken != dto.RefreshToken)
            {
                return Unauthorized("Érvénytelen refresh token.");
            }

            if (user.RefreshTokenExpiresAt == null || user.RefreshTokenExpiresAt <= DateTime.UtcNow)
            {
                return Unauthorized("Lejárt refresh token.");
            }

            var newRefreshToken = GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

            await ctx.SaveChangesAsync();

            return Ok(CreateAuthResponse(user));
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = User.FindFirstValue(ClaimTypes.Email);
            var name = User.FindFirstValue(ClaimTypes.Name);

            return Ok(new
            {
                userId,
                email,
                name
            });
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var user = await ctx.Users.SingleOrDefaultAsync(x => x.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            user.RefreshToken = null;
            user.RefreshTokenExpiresAt = null;

            await ctx.SaveChangesAsync();

            return Ok();
        }

        private AuthResponseDto CreateAuthResponse(User user)
        {
            var accessToken = GenerateAccessToken(user, out DateTime accessTokenExpiration);

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                AccessTokenExpiresAt = accessTokenExpiration,

                RefreshToken = user.RefreshToken ?? string.Empty,
                RefreshTokenExpiresAt = user.RefreshTokenExpiresAt ?? DateTime.UtcNow,

                UserId = user.Id,
                Email = user.Email,
                Name = user.Name,
                ProfilePictureUrl = user.ProfilePictureUrl ?? string.Empty
            };
        }

        private string GenerateAccessToken(User user, out DateTime expiration)
        {
            var jwtKey = config["Jwt:Key"];

            if (string.IsNullOrWhiteSpace(jwtKey))
            {
                throw new InvalidOperationException("Jwt:Key nincs beállítva.");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name)
            };

            var signingKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)
            );

            var signingCredentials = new SigningCredentials(
                signingKey,
                SecurityAlgorithms.HmacSha256
            );

            expiration = DateTime.UtcNow.AddMinutes(30);

            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                claims: claims,
                expires: expiration,
                signingCredentials: signingCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string GenerateRefreshToken()
        {
            var randomBytes = RandomNumberGenerator.GetBytes(64);

            return Convert.ToBase64String(randomBytes);
        }
    }
}
