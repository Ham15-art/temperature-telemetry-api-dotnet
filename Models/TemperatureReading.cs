namespace TemperatureApi.Models;

public class TemperatureReading
{
    public string DeviceId { get; set; } = string.Empty;
    public double Value { get; set; }
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
}
