using System.Collections.Concurrent;
using BatchProcessor.Common;
using BatchProcessor.Types;
using Microsoft.Extensions.Options;

namespace BatchProcessor.Services;

public class BatchJobProcessing : BackgroundService
{
    private readonly ConcurrentDictionary<Guid, Batch> _batches;
    private readonly CachingApi _cachingApi;
    private readonly ILogger<BatchJobProcessing> _logger;
    private readonly ConcurrentQueue<Batch> _queue;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public BatchJobProcessing(
        ILogger<BatchJobProcessing> logger,
        ConcurrentQueue<Batch> queue,
        IServiceScopeFactory serviceScopeFactory,
        IOptions<CachingApi> options)
    {
        _logger = logger;
        _queue = queue;
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
            Results = new List<string>(),
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

        foreach (var ipAddress in batch.IpAddresses)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var response = await httpClient.GetAsync($"{_cachingApi.BaseUrl}/getoradd/{ipAddress}");
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
                hasFailedIps = true;
                batch.Results.Add($"Error: {ipAddress} - {ex.Message}");
                batch.ProcessedCount++;
            }
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

            await Task.Delay(1000, cancellationToken);
        }
    }
}