using IPLookup.Common;
using IPLookup.Service;

namespace IPLookup.Services;

public class Lookup(IIpStackService ipStackService) : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapGet($"{Constants.GetDetails}{{ipAddress}}",Handle)
        .AddEndpointFilter<ValidateIPFilter>()
        .WithName("GetIPDetails")
        .WithOpenApi(op => new(op)
        {
            Summary = "Fetch IP Address Details",
            Description = "Retrieves geographical and other details for a provided IP address."
        });
    
    private static async Task<IResult> Handle(string ipAddress,IIpStackService ipStackService, CancellationToken cancellationToken)
    {
        var locationData = await ipStackService.GetLocationDataAsync(ipAddress, cancellationToken);

        return locationData == null 
            ? Results.BadRequest("Failed to fetch details for the IP address.") 
            : Results.Ok(locationData);
    }
}