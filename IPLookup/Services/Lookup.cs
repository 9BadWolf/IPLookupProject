using IPLookup.Common;
using IPLookup.Types;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.OpenApi.Models;

namespace IPLookup.Services;

public class Lookup : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app
            .MapGet("/api/ipdetails/{ipAddress}", Handle)
            .AddEndpointFilter<ValidateIPFilter>()
            .WithName("GetIPDetails")
            .WithOpenApi(op => new OpenApiOperation(op)
            {
                Summary = "Fetch IP Address Details",
                Description = "Retrieves geographical and other details for a provided IP address."
            });
    }

    private static async Task<Results<BadRequest<string>, Ok<IpDetails>>> Handle(string ipAddress,
        IIpStackService ipStackService, CancellationToken cancellationToken)
    {
        var locationData = await ipStackService.GetLocationDataAsync(ipAddress, cancellationToken);

        return locationData == null
            ? TypedResults.BadRequest("Failed to fetch details for the IP address.")
            : TypedResults.Ok(locationData);
    }
}