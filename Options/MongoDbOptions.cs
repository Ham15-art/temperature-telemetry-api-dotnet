namespace TemperatureApi.Options;

/// <summary>
/// Represents MongoDB configuration options.
/// </summary>
public class MongoDbOptions
{
    public const string SectionName = "MongoDb";

    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string CollectionName { get; set; } = string.Empty;

    /// <summary>
    /// Validates the MongoDB options at startup.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if configuration is invalid</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ConnectionString))
            throw new InvalidOperationException("MongoDb:ConnectionString is missing or empty. Configure via user-secrets or environment variables.");

        if (string.IsNullOrWhiteSpace(DatabaseName))
            throw new InvalidOperationException("MongoDb:DatabaseName is missing or empty.");

        if (string.IsNullOrWhiteSpace(CollectionName))
            throw new InvalidOperationException("MongoDb:CollectionName is missing or empty.");
    }
}
