using BatchProcessor.Common;
using BatchProcessor.Types;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.OpenApi.Models;

namespace BatchProcessor.Services;

public class GetStatus : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app
            .MapGet("/api/getstatus/{batchId}", Handle)
            .WithName("GetBatchStatus")
            .WithOpenApi(op => new OpenApiOperation(op)
            {
                Summary = "Get Batch Status",
                Description = "Check batch progress and results"
            });
    }

    private static async Task<Results<Ok<Batch>, NotFound<string>>> Handle(Guid batchId,
        BatchJobProcessing batchService)
    {
        var batchStatus = batchService.GetBatchStatus(batchId);
        return batchStatus == null
            ? TypedResults.NotFound("I can't find the provided batch ID")
            : TypedResults.Ok(batchStatus);
    }
}