using TemperatureApi.Models;

namespace TemperatureApi.Repositories;

public interface ITemperatureRepository
{
    Task SaveAsync(TemperatureReading reading, CancellationToken token = default);
}
