using BatchProcessor.Common;
using BatchProcessor.Services;

namespace BatchProcessor;

public static class Endpoints
{
    public static void MapEndpoints(this IEndpointRouteBuilder app)
    {
        var endpoints = app.MapGroup("")
            .WithOpenApi();
        endpoints.MapEndpoint<ProcessBatch>();
        endpoints.MapEndpoint<GetStatus>();
    }

    private static IEndpointRouteBuilder MapEndpoint<TEndpoint>(this IEndpointRouteBuilder app)
        where TEndpoint : IEndpoint
    {
        TEndpoint.Map(app);
        return app;
    }
}