using TemperatureApi.Models;

namespace TemperatureApi.Repositories;

public interface ITemperatureRepository
{
    Task SaveAsync(TemperatureReading reading, CancellationToken token = default);
    Task<List<TemperatureReading>> GetAllAsync(int limit=50);
    Task<TemperatureReading?> GetLatestAsync();
}
