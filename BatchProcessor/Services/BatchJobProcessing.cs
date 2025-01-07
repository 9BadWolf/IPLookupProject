using System.Collections.Concurrent;
using BatchProcessor.Common;
using BatchProcessor.Types;
using Microsoft.Extensions.Options;

namespace BatchProcessor.Services;

public class BatchJobProcessing: BackgroundService
{
    private readonly ILogger<BatchJobProcessing> _logger;
    private readonly ConcurrentQueue<BatchRequest> _queue;
    private readonly ConcurrentDictionary<Guid, Batch> _batches;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly CachingApi _cachingApi;

    public BatchJobProcessing(
        ILogger<BatchJobProcessing> logger,
        ConcurrentQueue<BatchRequest> queue,
        IServiceScopeFactory serviceScopeFactory,
        IOptions<CachingApi> options)
    {
        _logger = logger;
        _queue = queue;
        _batches = new ConcurrentDictionary<Guid, Batch>();
        _serviceScopeFactory = serviceScopeFactory;
        _cachingApi = options.Value;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_queue.TryDequeue(out var batchRequest))
            {
                var batchId = await CreateBatchAsync(batchRequest);
                await ProcessBatchAsync(batchId);
            }
            await Task.Delay(1000, stoppingToken); 
        }
    }
    private async Task<Guid> CreateBatchAsync(BatchRequest batchRequest)
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

        _batches[batchId] = batch;
        return batchId;
    }
    
    private async Task ProcessBatchAsync(Guid batchId)
    {
        if (!_batches.TryGetValue(batchId, out var batch))
        {
            _logger.LogError($"Batch with ID {batchId} not found.");
            return;
        }

        batch.Status = StatusEnum.InProgress;
        var failedIps = new List<string>();
        
        using var scope = _serviceScopeFactory.CreateScope();
        var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient();

        foreach (var ipAddress in batch.IpAddresses)
        {
            try
            {
                var response = await httpClient.GetAsync($"{_cachingApi.BaseUrl}/getoradd/{ipAddress}");
                if (response.IsSuccessStatusCode)
                {
                    batch.Results.Add($"Success: {ipAddress}");
                }
                else
                {
                    failedIps.Add(ipAddress);
                    batch.Results.Add($"Failed: {ipAddress} - {response.StatusCode}");
                }

                batch.ProcessedCount++;
            }
            catch (Exception ex)
            {
                failedIps.Add(ipAddress);
                batch.Results.Add($"Error: {ipAddress} - {ex.Message}");
                batch.ProcessedCount++;
            }
        }

        batch.Status = failedIps.Count == 0 ? StatusEnum.Completed : StatusEnum.Failed;
        _batches[batchId] = batch;
    }

    public Batch GetBatchStatus(Guid batchId)
    {
        _batches.TryGetValue(batchId, out var batch);
        return batch;
    }
}