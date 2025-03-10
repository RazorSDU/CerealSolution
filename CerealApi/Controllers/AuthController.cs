using Microsoft.AspNetCore.Mvc;
using CerealApi.Data;
using CerealApi.Models;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;

namespace CerealApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly CerealContext _context;
        private readonly IConfiguration _config;

        public AuthController(CerealContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        /// <summary>
        /// Register a new user (password is hashed).
        /// Example body:
        /// {
        ///   "username": "admin",
        ///   "password": "MySecretPass123"
        /// }
        /// </summary>
        [HttpPost("register")]
        public IActionResult Register([FromBody] UserRegisterDto request)
        {
            // 1) Check if user already exists
            if (_context.Users.Any(u => u.Username == request.Username))
                return BadRequest("Username already taken.");

            // 2) Hash password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // 3) Create user
            var user = new User
            {
                Username = request.Username,
                PasswordHash = passwordHash,
                Role = "Admin" // or "User" depending on your design
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return Ok("User created successfully!");
        }

        /// <summary>
        /// Login with existing user credentials,
        /// returns JWT if valid.
        /// </summary>
        [HttpPost("login")]
        public IActionResult Login([FromBody] UserLoginDto request)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == request.Username);
            if (user == null)
                return Unauthorized("Invalid username or password.");

            // Check password
            bool validPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!validPassword)
                return Unauthorized("Invalid username or password.");

            // Generate JWT
            var token = GenerateToken(user);
            return Ok(new { token });
        }

        private string GenerateToken(User user)
        {
            var jwtSettings = _config.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Add whatever claims you want
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.Now.AddHours(3), // token valid for 3 hours
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    // Request DTOs to avoid exposing entire User object
    public class UserRegisterDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class UserLoginDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
