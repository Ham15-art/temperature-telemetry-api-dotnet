using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TemperatureApi.Models;
using TemperatureApi.Repositories;

namespace TemperatureApi.Controllers;

[ApiController]
[Route("temperature")]

public class TelemetryController : ControllerBase
{
    //inject logger
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
        _logger.LogInformation("DeviceId: {deviceId}", reading.DeviceId);
        _logger.LogInformation("Temperature value: {value}", reading.Value);
        _logger.LogInformation("Timestamp: {timestamp}", reading.TimestampUtc);

        if (reading == null)
        {
            _logger.LogWarning("Request body is null");
            return BadRequest("No data received");
        }
        if (string.IsNullOrWhiteSpace(reading.DeviceId))
        {
            _logger.LogWarning("Validation failed: DeviceId missing");
            return BadRequest(new ErrorResponse { Error = "DeviceId is required" });
        }
        if (reading.Value < -50 || reading.Value > 150)
        {
            _logger.LogWarning("Validation failed: Temperature out of range");
            return BadRequest("Temperature out of defined range");
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
    public async Task<IActionResult> GetAllReadings([FromQuery] int limit=50)
    {
        _logger.LogInformation("Fetching temperature readings, limit={limit}", limit);
        var readings = await _repo.GetAllAsync(limit);

        if (readings != null)
            return Ok(readings);
        return NotFound("No readings found");
    }

    [HttpGet("latest")]
    [Authorize]
    public async Task<IActionResult> GetLatestReading()
    {
        _logger.LogInformation("Fetching latest temperature reading");
        var reading = await _repo.GetLatestAsync();

        if (reading != null)
            return Ok(reading);
        return NotFound("No readings found");
    }
}
