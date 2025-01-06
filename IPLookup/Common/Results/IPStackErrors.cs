using System.Net;
using System.Text.Json;
using IPLookup.Types;
using Serilog;

namespace IPLookup.Common.Results;

public interface IIpStackErrorHandler
{
    void HandleApiError(HttpStatusCode statusCode, string content, string ipAddress);
}

public class IpStackErrorHandler : IIpStackErrorHandler
{
    public void HandleApiError(HttpStatusCode statusCode, string content, string ipAddress)
    {
        try
        {
            var response = JsonSerializer.Deserialize<IpStackError>(content);

            if (response is { Success: false })
            {
                switch (response.Error.Code)
                {
                    case 404:
                        Log.Warning("IPStack API: Resource not found for IP: {IPAddress}", ipAddress);
                        break;
                    case 101:
                       Log.Error("IPStack API: Missing access key.");
                        throw new ExternalServiceException("No API Key was specified or an invalid API Key was specified.");
                    case 102:
                       Log.Error("IPStack API: User account is inactive. Contact customer support.");
                        throw new ExternalServiceException("User account is inactive.");
                    case 103:
                       Log.Error("IPStack API: Invalid API endpoint requested.");
                        throw new ExternalServiceException("Invalid API endpoint.");
                    case 104:
                      Log.Error("IPStack API: Monthly usage limit reached.");
                        throw new ExternalServiceException("Monthly usage limit reached.");
                    default:
                      Log.Warning("IPStack API returned an unhandled error code: {ErrorCode} for IP: {IPAddress}", response.Error.Code, ipAddress);
                        break;
                }
            }
            else
            {
                Log.Warning("IPStack API returned a non-success status code ({StatusCode}) but no error code was found in the response for IP: {IPAddress}", statusCode, ipAddress);
            }
        }
        catch (JsonException ex)
        {
            Log.Error(ex, "Failed to parse error response from IPStack API for IP: {IPAddress}", ipAddress);
        }
    }
}