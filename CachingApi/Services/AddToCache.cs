using Cache.Common;
using IPLookup.Types;
using Microsoft.AspNetCore.Mvc;

namespace CachingApi.Services;

public class AddToCache : IEndpoint
{
     public static void Map(IEndpointRouteBuilder app) => app
        .MapPut($"{Constants.AddToCache}{{ipAddress}}", async (string ipAddress,[FromBody] IpDetails value, CacheService cache) =>
            await cache.AddCacheAsync(ipAddress, value))
        .WithName("AddCachedIpDetails")
        .WithOpenApi(op => new(op)
        {
            Summary = "Add IP Address Details",
            Description = "Adds geographical and other details for a provided IP address to cache."
        });
}
