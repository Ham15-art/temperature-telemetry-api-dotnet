using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TemperatureApi.Models;
using TemperatureApi.Repositories;

namespace TemperatureApi.Controllers;

[ApiController]
[Route("temperature")]
public class TelemetryController : ControllerBase
{
    private readonly ILogger<TelemetryController> _logger;
    private readonly ITemperatureRepository _repo;

    public TelemetryController(ILogger<TelemetryController> logger, ITemperatureRepository repo)
    {
        _logger = logger;
        _repo = repo;
    }

    [HttpPost]
    public async Task<IActionResult> ReceiveTemperature(
        [FromBody] TemperatureReading reading,
        CancellationToken token
    )
    {
        _logger.LogInformation("Request Received");

        // Validate null body first before accessing properties
        if (reading == null)
        {
            _logger.LogWarning("Validation failed: Request body is null");
            return BadRequest(new ErrorResponse { Error = "No data received" });
        }

        _logger.LogInformation("DeviceId: {deviceId}", reading.DeviceId);
        _logger.LogInformation("Temperature value: {value}", reading.Value);
        _logger.LogInformation("Timestamp: {timestamp}", reading.TimestampUtc);

        // Validate input
        var validationError = ValidateTemperatureReading(reading);
        if (validationError != null)
        {
            _logger.LogWarning("Validation failed: {error}", validationError);
            return BadRequest(new ErrorResponse { Error = validationError });
        }

        _logger.LogInformation("Temperature reading accepted");

        await _repo.SaveAsync(reading, token);
        return Ok(
            new
            {
                status = "received",
                message = "Temperature reading is accepted",
                data = reading,
            }
        );
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAllReadings([FromQuery] int limit = 50)
    {
        _logger.LogInformation("Fetching temperature readings, limit={limit}", limit);

        if (limit <= 0 || limit > 1000)
        {
            return BadRequest(new ErrorResponse { Error = "Limit must be between 1 and 1000" });
        }

        var readings = await _repo.GetAllAsync(limit);

        if (readings != null && readings.Count > 0)
            return Ok(readings);
        
        return NotFound(new ErrorResponse { Error = "No readings found" });
    }

    [HttpGet("latest")]
    [Authorize]
    public async Task<IActionResult> GetLatestReading()
    {
        _logger.LogInformation("Fetching latest temperature reading");
        var reading = await _repo.GetLatestAsync();

        if (reading != null)
            return Ok(reading);
        
        return NotFound(new ErrorResponse { Error = "No readings found" });
    }

    /// <summary>
    /// Validates temperature reading against business rules.
    /// </summary>
    /// <returns>Error message if validation fails; null if valid</returns>
    private string? ValidateTemperatureReading(TemperatureReading reading)
    {
        if (string.IsNullOrWhiteSpace(reading.DeviceId))
            return "DeviceId is required";

        if (reading.Value < -50 || reading.Value > 150)
            return "Temperature must be between -50°C and 150°C";

        if (reading.TimestampUtc > DateTime.UtcNow.AddSeconds(30))
            return "Timestamp cannot be in the future";

        return null;
    }
}

