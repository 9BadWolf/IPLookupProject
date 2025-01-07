using Cache.Common;
using Microsoft.Extensions.Caching.Memory;

namespace CachingApi.Services;

public class GetOrAdd :IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapGet($"{Constants.GetOrAdd}{{iPAddress}}", async (string iPAddress, CacheService cache) =>
            await cache.GetOrAddCacheAsync(iPAddress))
        .WithName("GetCachedIpDetails")
        .WithOpenApi(op => new(op)
        {
            Summary = "Get IP Address Details",
            Description = "Retrieves geographical and other details for a provided IP address."
        });
}