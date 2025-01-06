using IPLookup.Common;
using IPLookup.Service;

namespace IPLookup.Services;

public class Lookup(IIpStackService ipStackService) : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapGet("get-ip-details/{ipAddress}",
            async (string ipAddress, Lookup lookup, CancellationToken cancellationToken) =>
            {
                return await lookup.Handle(ipAddress, cancellationToken);
            })
        .AddEndpointFilter<ValidateIPFilter>()
        .WithName("GetIPDetails")
        .WithOpenApi(op => new(op)
        {
            Summary = "Fetch IP Address Details",
            Description = "Retrieves geographical and other details for a provided IP address."
        });
    
    private async Task<IResult> Handle(string ipAddress, CancellationToken cancellationToken)
    {
        var locationData = await ipStackService.GetLocationDataAsync(ipAddress, cancellationToken);

        return locationData == null 
            ? Results.BadRequest("Failed to fetch details for the IP address.") 
            : Results.Ok(locationData);
    }
}