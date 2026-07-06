using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TemperatureApi.Models;

namespace TemperatureApi.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;

    // Hardcoded for now — in a real app, load users from MongoDB
    private const string AdminUsername = "admin";
    private readonly string _adminPasswordHash =
        BCrypt.Net.BCrypt.HashPassword("admin123");

    public AuthController(IConfiguration config)
    {
        _config = config;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        bool usernameMatch = request.Username == AdminUsername;
        bool passwordMatch = BCrypt.Net.BCrypt.Verify(request.Password, _adminPasswordHash);

        if (!usernameMatch || !passwordMatch)
            return Unauthorized(new { error = "Invalid credentials" });

        var token = GenerateToken(request.Username);
        return Ok(new { token });
    }

    private string GenerateToken(string username)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, "Operator")
        };

        var expiry = int.Parse(_config["Jwt:ExpiryMinutes"]!);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiry),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}