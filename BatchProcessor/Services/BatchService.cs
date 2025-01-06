using System.Collections.Concurrent;
using BatchProcessor.Types;
using Hangfire;

namespace BatchProcessor.Services;

public class BatchService
{
    private readonly RedisService _redisService;
    private readonly IBackgroundJobClient _backgroundJobs;
    
    public BatchService(RedisService redisService, IBackgroundJobClient backgroundJobs)
    {
        _redisService = redisService;
        _backgroundJobs = backgroundJobs;
    }
    public async Task<Guid> CreateBatchAsync(BatchRequest batchRequest)
    {
        var batchId = Guid.NewGuid();
        var batch = new Batch
        {
            BatchId = batchId,
            IpAddresses = batchRequest.IpAddresses,
            Status = StatusEnum.Pending,
            Results = new List<string>(),
            TotalCount = batchRequest.IpAddresses.Count
        };

        await _redisService.SetBatchAsync(batchId, batch);
        _backgroundJobs.Enqueue(() => ProcessBatchAsync(batchId));

        return batchId;
    }

    public async Task<Batch?> GetBatchStatusAsync(Guid batchId)
    {
        return await _redisService.GetBatchAsync(batchId);
    }

    public async Task ProcessBatchAsync(Guid batchId)
    {
        var batch = await _redisService.GetBatchAsync(batchId);
        if (batch == null) return;

        batch.Status = StatusEnum.InProgress;
        await _redisService.SetBatchAsync(batchId, batch);


        foreach (var ip in batch.IpAddresses.Take(10))
        {
            var ipDetails = $"Details for {ip}"; // TODO add the get details
            batch.Results.Add(ipDetails);
            batch.ProcessedCount++;

            await _redisService.SetBatchAsync(batchId, batch);
        }

        batch.Status = StatusEnum.Completed;
        await _redisService.SetBatchAsync(batchId, batch);
    }
}
