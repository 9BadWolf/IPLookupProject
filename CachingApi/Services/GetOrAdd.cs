using Cache.Common;
using CachingApi.Types;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.OpenApi.Models;

namespace CachingApi.Services;

public class GetOrAdd : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app
            .MapGet("/api/getoradd/{ipAddress}", Handle)
            .WithName("GetCachedIpDetails")
            .WithOpenApi(op => new OpenApiOperation(op)
            {
                Summary = "Get IP Address Details",
                Description = "Retrieves geographical and other details for a provided IP address."
            });
    }

    private static async Task<Results<NotFound<string>, Ok<IpDetails>>> Handle(string ipAddress, CacheService cache,
        CancellationToken cancellationToken)
    {
        var cachedItem = await cache.GetOrAddCacheAsync(ipAddress);

        return cachedItem.Result switch
        {
            NotFound<string> notFound => TypedResults.NotFound(notFound.Value),
            Ok<IpDetails> ok => TypedResults.Ok(ok.Value),
            _ => throw new InvalidOperationException("Unexpected result type.")
        };
    }
}