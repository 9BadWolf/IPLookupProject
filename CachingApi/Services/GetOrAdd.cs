using Cache.Common;
using IPLookup.Types;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CachingApi.Services;

public class GetOrAdd : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapGet("/api/getoradd/{ipAddress}", Handle)
        .WithName("GetCachedIpDetails")
        .WithOpenApi(op => new(op)
        {
            Summary = "Get IP Address Details",
            Description = "Retrieves geographical and other details for a provided IP address."
        });

    private static async Task<Results<BadRequest<string>, Ok<IpDetails>>> Handle(string ipAddress, CacheService cache,
        CancellationToken cancellationToken)
    {
        var cachedItem = await cache.GetOrAddCacheAsync(ipAddress);

        return cachedItem.Result switch
        {
            BadRequest<string> badRequest => TypedResults.BadRequest(badRequest.Value),
            Ok<IpDetails> ok => TypedResults.Ok(ok.Value),
            _ => throw new InvalidOperationException("Unexpected result type.")
        };
    }
}
