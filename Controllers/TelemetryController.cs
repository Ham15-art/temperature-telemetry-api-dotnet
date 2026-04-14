using Microsoft.AspNetCore.Mvc;
using TemperatureApi.Models;

namespace TemperatureApi.Controllers;

[ApiController]
[Route("temperature")]
public class TelemetryController : ControllerBase
{
    //inject logger
    private readonly ILogger<TelemetryController> _logger;
    public TelemetryController(ILogger<TelemetryController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    public IActionResult ReceiveTemperature([FromBody] TemperatureReading reading)
    {
        _logger.LogInformation("Request Received");
        _logger.LogInformation("DeviceId: {deviceId}", reading.DeviceId);
        _logger.LogInformation("Temperature value: {value}", reading.Value);
        _logger.LogInformation("Timestamp: {timestamp}", reading.TimestampUtc);

        if (reading == null)
        {
            return BadRequest("No data received");
        }
        if (string.IsNullOrWhiteSpace(reading.DeviceId))
        {
            _logger.LogWarning("Validation failed: DeviceId missing");
            return BadRequest(new { error = "DeviceId is required" });
        }
        if (reading.Value < -50 || reading.Value > 150)
        {
            _logger.LogWarning("Validation failed: Temperature out of range");
            return BadRequest("Temperature out of defined range");
        }

        _logger.LogInformation("Temperature reading accepted");
        return Ok(
            new
            {
                status = "received",
                message = "Temperature reading is accepted",
                data = reading,
            }
        );
    }
}
