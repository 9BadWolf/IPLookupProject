using Cache.Common;
using IPLookup.Types;
using Microsoft.AspNetCore.Mvc;

namespace CachingApi.Services;

public class AddToCache : IEndpoint
{
     public static void Map(IEndpointRouteBuilder app) => app
        .MapPut("/cache/add/{key}", async (string key,[FromBody] IpDetails value, CacheService cache) =>
            await cache.AddCacheAsync(key, value))
        .WithName("AddCachedIpDetails")
        .WithOpenApi(op => new(op)
        {
            Summary = "Add IP Address Details",
            Description = "Adds geographical and other details for a provided IP address to cache."
        });
}
