using Cache.Common;
using IPLookup.Types;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

namespace CachingApi.Services;

public class AddToCache : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPut("/api/addtocache/{ipAddress}",Handle)
        .WithName("AddCacheIpDetails")
        .WithOpenApi(op => new(op)
        {
            Summary = "Add IP Address Details",
            Description = "Adds geographical and other details for a provided IP address to cache."
        });


    private static async Task<Results<BadRequest<string>, Ok<IpDetails>>> Handle(string ipAddress,
        [FromBody] IpDetails value, CacheService cache, CancellationToken cancellationToken)
    {
        var cachedItem = await cache.AddCacheAsync(ipAddress, value);

        return cachedItem == null
            ? TypedResults.BadRequest("Failed to add to cache.")
            : TypedResults.Ok(cachedItem);
    }
}