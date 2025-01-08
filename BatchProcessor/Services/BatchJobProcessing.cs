using System.Collections.Concurrent;
using BatchProcessor.Common;
using BatchProcessor.Types;
using Microsoft.Extensions.Options;

namespace BatchProcessor.Services;

public class BatchJobProcessing : BackgroundService
{
    private const int ChunkSize = 10;
    private const int DelayDequeueing = 1;
    private const int DelayChunks = 1;
    private readonly ConcurrentDictionary<Guid, Batch> _batches;
    private readonly CachingApi _cachingApi;
    private readonly ILogger<BatchJobProcessing> _logger;
    private readonly ConcurrentQueue<Batch> _queue;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public BatchJobProcessing(
        ILogger<BatchJobProcessing> logger,
        IServiceScopeFactory serviceScopeFactory,
        IOptions<CachingApi> options)
    {
        _logger = logger;
        _queue = new ConcurrentQueue<Batch>();
        _batches = new ConcurrentDictionary<Guid, Batch>();
        _serviceScopeFactory = serviceScopeFactory;
        _cachingApi = options.Value;
    }

    public async Task<Guid> CreateBatchAsync(BatchRequest batchRequest, CancellationToken cancellationToken)
    {
        var batchId = Guid.NewGuid();
        var batch = new Batch
        {
            BatchId = batchId,
            IpAddresses = batchRequest.IpAddresses,
            Status = StatusEnum.Pending,
            Results = [],
            TotalCount = batchRequest.IpAddresses.Count
        };

        _batches[batchId] = batch;
        _queue.Enqueue(batch);

        return batchId;
    }

    public Batch? GetBatchStatus(Guid batchId)
    {
        _batches.TryGetValue(batchId, out var batch);
        return batch;
    }

    private async Task ProcessBatchAsync(Batch batch, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Started processing batch with {BatchId}", batch.BatchId);

        batch.Status = StatusEnum.InProgress;
        var hasFailedIps = false;

        using var scope = _serviceScopeFactory.CreateScope();
        var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient();
        var chunks = batch.IpAddresses.Chunk(ChunkSize).ToList();
        
        foreach (var chunk in chunks)
        {
            foreach (var ipAddress in chunks)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    var response = await httpClient.GetAsync($"{_cachingApi.BaseUrl}/getoradd/{ipAddress}", cancellationToken);
                    if (response.IsSuccessStatusCode)
                    {
                        batch.Results.Add($"Success: {ipAddress}");
                    }
                    else
                    {
                        hasFailedIps = true;
                        batch.Results.Add($"Failed: {ipAddress} - {response.StatusCode}");
                    }

                    batch.ProcessedCount++;
                }
            
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Something went wrong");
                    hasFailedIps = true;
                    batch.Results.Add($"Error: {ipAddress} - {ex.Message}");
                    batch.ProcessedCount++;
                }
            }
            
            await Task.Delay(TimeSpan.FromMinutes(DelayChunks), cancellationToken);
        }

        _logger.LogInformation("Successfully processed batch with {BatchId}", batch.BatchId);

        batch.Status = hasFailedIps ? StatusEnum.Failed : StatusEnum.Completed;
        _batches[batch.BatchId] = batch;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (_queue.TryDequeue(out var batch))
            {
                _logger.LogInformation("Processing batch with ID: {BatchId}", batch.BatchId);
                await ProcessBatchAsync(batch, cancellationToken);
            }
            else
            {
                _logger.LogWarning("No batches in the queue to process.");
            }

            await Task.Delay(TimeSpan.FromMinutes(DelayDequeueing), cancellationToken);
        }
    }
}