using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using Polly;
using TemperatureApi.Models;
using TemperatureApi.Options;

namespace TemperatureApi.Repositories;

public class MongoTemperatureRepository : ITemperatureRepository
{
    private readonly IMongoCollection<TemperatureReading> _collection;
    private readonly ILogger<MongoTemperatureRepository> _logger;
    private readonly IMongoClient _mongoClient;
    private bool _indexesCreated = false;

    public MongoTemperatureRepository(
        MongoDbOptions mongoDbOptions,
        ILogger<MongoTemperatureRepository> logger
    )
    {
        _logger = logger;

        _mongoClient = new MongoClient(mongoDbOptions.ConnectionString);
        var database = _mongoClient.GetDatabase(mongoDbOptions.DatabaseName);
        _collection = database.GetCollection<TemperatureReading>(mongoDbOptions.CollectionName);

        // Create indexes asynchronously in background
        _ = InitializeIndexesAsync();
    }

    public async Task SaveAsync(TemperatureReading reading, CancellationToken token = default)
    {
        try
        {
            await _collection.InsertOneAsync(reading, cancellationToken: token);

            _logger.LogInformation(
                "Saved reading to MongoDB for {DeviceId} with value {Value}°C",
                reading.DeviceId,
                reading.Value
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save reading for {DeviceId}", reading.DeviceId);
            throw;
        }
    }

    public async Task<List<TemperatureReading>> GetAllAsync(int limit = 50)
    {
        try
        {
            return await _collection
                .Find(_ => true)
                .SortByDescending(x => x.TimestampUtc)
                .Limit(limit)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve all readings");
            throw;
        }
    }

    public async Task<TemperatureReading?> GetLatestAsync()
    {
        try
        {
            return await _collection
                .Find(_ => true)
                .SortByDescending(x => x.TimestampUtc)
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve latest reading");
            throw;
        }
    }

    /// <summary>
    /// Creates necessary indexes on the collection for query performance.
    /// </summary>
    private async Task InitializeIndexesAsync()
    {
        try
        {
            if (_indexesCreated)
                return;

            _logger.LogInformation("Creating MongoDB indexes for TemperatureReading collection");

            // Index on TimestampUtc for sorting and range queries
            var timestampIndexModel = new CreateIndexModel<TemperatureReading>(
                Builders<TemperatureReading>.IndexKeys.Descending(x => x.TimestampUtc),
                new CreateIndexOptions { Name = "idx_timestampUtc" }
            );

            // Index on DeviceId for filtering by device
            var deviceIdIndexModel = new CreateIndexModel<TemperatureReading>(
                Builders<TemperatureReading>.IndexKeys.Ascending(x => x.DeviceId),
                new CreateIndexOptions { Name = "idx_deviceId" }
            );

            // Compound index for common query patterns
            var compoundIndexModel = new CreateIndexModel<TemperatureReading>(
                Builders<TemperatureReading>.IndexKeys
                    .Ascending(x => x.DeviceId)
                    .Descending(x => x.TimestampUtc),
                new CreateIndexOptions { Name = "idx_deviceId_timestampUtc" }
            );

            await _collection.Indexes.CreateManyAsync(new[] { timestampIndexModel, deviceIdIndexModel, compoundIndexModel });

            _indexesCreated = true;
            _logger.LogInformation("MongoDB indexes created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create indexes; queries may be slower. This is non-critical.");
            // Don't rethrow — index creation failure should not prevent the application from running
        }
    }
}
