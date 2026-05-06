using DiplomaFit.Data;
using DiplomaFit.Model.Dto.Auth;
using DiplomaFit.Model.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FitAppBackend.Api.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext ctx;
        private readonly IConfiguration config;
        private readonly PasswordHasher<User> passwordHasher = new();

        public AuthController(AppDbContext ctx, IConfiguration config)
        {
            this.ctx = ctx;
            this.config = config;
        }

        [HttpPost("login")]
        public IActionResult Login(LoginRequestDto dto)
        {
            var user = ctx.Users.SingleOrDefault(x => x.Email == dto.Email);
            if (user == null)
                return Unauthorized("Nincs ilyen felhasználó");


            var passwordHasher = new PasswordHasher<User>();
            user.PasswordHash = passwordHasher.HashPassword(user, "jelszo123");
            ctx.SaveChanges();

            var result = passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            dto.Password);

            if (result == PasswordVerificationResult.Failed)
                return Unauthorized("Hibás jelszó");

            var token = GenerateJwt(user);
            return Ok(new LoginResponseDto
            {
                Token = token
            });

        }

        private string GenerateJwt(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(config["Jwt:Key"]!)
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            return Ok(email);
        }

    }
}
