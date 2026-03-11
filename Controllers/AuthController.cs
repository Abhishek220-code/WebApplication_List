using Azure;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WebApplication_List.Models;

[Route("auth")]
public class AuthController : Controller
{
    private readonly IConfiguration _config;
    private readonly AppDbContext _context;

    public AuthController(IConfiguration config, AppDbContext context)
    {
        _config = config;
        _context = context;
    }



    [HttpPost("SignupUser")]
    public async Task<IActionResult> SignupUser(string Username, string Password)
    {

        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            return BadRequest(new { message = "Username and password are required." });
        }

        bool userExists = await _context.Users
                                        .AnyAsync(u => u.Username == Username);

        if (userExists)
        {
            return BadRequest(new { message = "Username already exists." });
        }

        var users = new Users
        {
            User_ID = Guid.NewGuid(),
            Username = Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(Password)
        };

        _context.Users.Add(users);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Signup successful! You can now log in." });

    }


    [HttpPost("Login")]
    public async Task<IActionResult> Login(string Username, string Password, bool RememberMe)
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            return Unauthorized(new { message = "Invalid username or password" });
        }

        try
        {
            var user = await _context.Users
                                     .FirstOrDefaultAsync(u => u.Username == Username);

            if (user == null)
                return Unauthorized(new { message = "Invalid username or password" });

            bool isValidPassword = BCrypt.Net.BCrypt.Verify(Password, user.PasswordHash);
            if (!isValidPassword)
                return Unauthorized(new { message = "Invalid username or password" });

            var tokenExpiry = RememberMe
                ? DateTime.UtcNow.AddDays(7)
                : DateTime.UtcNow.AddMinutes(30);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.User_ID.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, Username)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var jwtToken = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: tokenExpiry,
                signingCredentials: creds
            );

            string token = new JwtSecurityTokenHandler().WriteToken(jwtToken);

            // 🍪 Set cookie
            Response.Cookies.Append("JWTToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Expires = RememberMe ? tokenExpiry : null
            });


            return Ok(new { message = "Login successful" });
        }
        catch (Exception ex)
        {
            // 🔍 Return full error temporarily for debugging
            return StatusCode(500, new { message = ex.Message, inner = ex.InnerException?.Message });

        }
    }
}


