using System.Text.Json;
using BatchProcessor.Types;
using StackExchange.Redis;

namespace BatchProcessor.Services;

public class RedisService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;

    public RedisService(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _db = _redis.GetDatabase();
    }

    public async Task<Batch?> GetBatchAsync(Guid batchId)
    {
        var batchJson = await _db.StringGetAsync(batchId.ToString());
        if (batchJson.IsNullOrEmpty) return null;

        return JsonSerializer.Deserialize<Batch>(batchJson);
    }

    public async Task SetBatchAsync(Guid batchId, Batch batch)
    {
        await _db.StringSetAsync(batchId.ToString(), JsonSerializer.Serialize(batch));
    }
}