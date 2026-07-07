using System;
namespace TemperatureApi.Options;

/// <summary>
/// Represents JWT configuration options.
/// </summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; } = 60;

    /// <summary>
    /// Validates the JWT options at startup.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if configuration is invalid</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Key))
            throw new InvalidOperationException("Jwt:Key is missing or empty. Configure via user-secrets or environment variables.");

        if (Key.Length < 32)
            throw new InvalidOperationException("Jwt:Key must be at least 32 characters long for security.");

        if (string.IsNullOrWhiteSpace(Issuer))
            throw new InvalidOperationException("Jwt:Issuer is missing or empty.");

        if (string.IsNullOrWhiteSpace(Audience))
            throw new InvalidOperationException("Jwt:Audience is missing or empty.");

        if (ExpiryMinutes <= 0)
            throw new InvalidOperationException("Jwt:ExpiryMinutes must be greater than 0.");
    }
}
