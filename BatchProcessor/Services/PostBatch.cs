using BatchProcessor.Common;
using BatchProcessor.Types;
using Microsoft.AspNetCore.Mvc;

namespace BatchProcessor.Services;

public class PostBatch :IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost($"{Constants.PostBatch}", async (BatchService batchService, [FromBody] BatchRequest batchRequest) =>
        {
            var batchId = await batchService.CreateBatchAsync(batchRequest);
            return Results.Ok(new { BatchId = batchId });
        })
        .WithName("ProcessBatch")
        .WithOpenApi(op => new(op)
        {
            Summary = "Add many IP Address Details",
            Description = "Add many IP Address Details in cache"
        });
}