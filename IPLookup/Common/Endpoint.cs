namespace IPLookup.Common;

public interface IEndpoint
{
    static abstract void Map(IEndpointRouteBuilder app);
}