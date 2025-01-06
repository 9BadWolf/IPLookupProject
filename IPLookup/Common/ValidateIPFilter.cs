using IPLookup.Common.Results;

namespace IPLookup.Common;

public class ValidateIPFilter: IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var ipAddress = context.GetArgument<string>(0);
        if (!TryParseIPv4(ipAddress, out _))
        {
            return new InvalidProblem("Invalid IP Address.");
        }
        return await next(context);
    }

    private bool TryParseIPv4(string? ipString, out string validIp)
    {
        validIp = string.Empty;

        if (string.IsNullOrWhiteSpace(ipString))
        {
            return false;
        }

        var splitValues = ipString.Trim().Split('.');
        if (splitValues.Length != 4)
        {
            return false;
        }

        if (splitValues.Any(segment => !byte.TryParse(segment, out _) || (segment.Length > 1 && segment.StartsWith("0"))))
        {
            return false;
        }

        validIp = ipString.Trim();
        return true;
    }
}