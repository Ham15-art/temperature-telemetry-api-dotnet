using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TemperatureApi.Models;
using TemperatureApi.Options;

namespace TemperatureApi.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly JwtOptions _jwtOptions;
    private readonly ILogger<AuthController> _logger;

    // Hardcoded for now — in a real app, load users from MongoDB
    private const string AdminUsername = "admin";
    private readonly string _adminPasswordHash =
        BCrypt.Net.BCrypt.HashPassword("admin123");

    public AuthController(IConfiguration config, JwtOptions jwtOptions, ILogger<AuthController> logger)
    {
        _config = config;
        _jwtOptions = jwtOptions;
        _logger = logger;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            _logger.LogWarning("Login attempt with missing credentials");
            return BadRequest(new ErrorResponse { Error = "Username and password are required" });
        }

        bool usernameMatch = request.Username == AdminUsername;
        bool passwordMatch = BCrypt.Net.BCrypt.Verify(request.Password, _adminPasswordHash);

        if (!usernameMatch || !passwordMatch)
        {
            _logger.LogWarning("Failed login attempt for username: {username}", request.Username);
            return Unauthorized(new { error = "Invalid credentials" });
        }

        _logger.LogInformation("Successful login for user: {username}", request.Username);
        var token = GenerateToken(request.Username);
        return Ok(new { token });
    }

    private string GenerateToken(string username)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, "Operator")
        };

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiryMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}