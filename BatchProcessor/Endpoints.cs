using BatchProcessor.Common;

namespace BatchProcessor;

public static class Endpoints
{
    public static void MapEndpoints(this IEndpointRouteBuilder app)
    {
        var endpoints = app.MapGroup("")
            .WithOpenApi();
       // endpoints.MapEndpoint<Lookup>();
    }
    private static IEndpointRouteBuilder MapEndpoint<TEndpoint>(this IEndpointRouteBuilder app) where TEndpoint : IEndpoint
    {
        TEndpoint.Map(app);
        return app;
    }
}