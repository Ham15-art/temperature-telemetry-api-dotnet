using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using TemperatureApi.Models;

namespace TemperatureApi.Repositories;

public class MongoTemperatureRepository : ITemperatureRepository
{
    private readonly IMongoCollection<TemperatureReading> _collection;
    private readonly ILogger<MongoTemperatureRepository> _logger;

    public MongoTemperatureRepository(
        IConfiguration configuration,
        ILogger<MongoTemperatureRepository> logger
    )
    {
        _logger = logger;

        var connectionString =
            configuration["MongoDb:ConnectionString"]
            ?? throw new InvalidOperationException("MongoDb:ConnectionString is missing.");

        var databaseName =
            configuration["MongoDb:DatabaseName"]
            ?? throw new InvalidOperationException("MongoDb:DatabaseName is missing.");

        var collectionName =
            configuration["MongoDb:CollectionName"]
            ?? throw new InvalidOperationException("MongoDb:CollectionName is missing.");

        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(databaseName);
        _collection = database.GetCollection<TemperatureReading>(collectionName);
    }

    public async Task SaveAsync(TemperatureReading reading, CancellationToken token = default)
    {
        await _collection.InsertOneAsync(reading, cancellationToken: token);

        _logger.LogInformation(
            "Saved reading to MongoDB for {DeviceId} with value {Value}",
            reading.DeviceId,
            reading.Value
        );
    }
}
