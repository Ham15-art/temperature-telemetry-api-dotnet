using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TemperatureApi.Models;

public class TemperatureReading
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public double Value { get; set; }
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
}
