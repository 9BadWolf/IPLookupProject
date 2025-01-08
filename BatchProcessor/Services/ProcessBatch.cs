using BatchProcessor.Common;
using BatchProcessor.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

namespace BatchProcessor.Services;

public class ProcessBatch : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app
            .MapPost("/api/processbatch/", Handle)
            .WithName("ProcessBatch")
            .WithOpenApi(op => new OpenApiOperation(op)
            {
                Summary = "Add many IP Address Details",
                Description = "Add many IP Address Details in cache asynchronously. Returns Batch Id to check progress"
            });
    }

    private static async Task<IResult> Handle( BatchRequest batchRequest,
         IEnumerable<IHostedService> services, CancellationToken cancellationToken)
    {
        var batchService = services.OfType<BatchJobProcessing>().First();
        var batchId = await batchService.CreateBatchAsync(batchRequest, cancellationToken);

        return TypedResults.Ok(new { BatchId = batchId });
    }
}