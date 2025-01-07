using System.Text.Json;
using IPLookup.Common;
using IPLookup.Common.Results;
using IPLookup.Types;
using Microsoft.Extensions.Options;
using Serilog;

namespace IPLookup.Service;

public interface IIpStackService
{
    Task<IpDetails?> GetLocationDataAsync(string ipAddress, CancellationToken cancellationToken);
}

public class IpStackService(
    IHttpClientFactory httpClientFactory,
    IOptions<IpStack> apiSettings,
    IIpStackErrorHandler errorHandler)
    : IIpStackService
{

    private readonly string _baseUrl = apiSettings.Value.BaseUrl;
    private readonly string _apiKey = apiSettings.Value.AccessKey;

    public async Task<IpDetails?> GetLocationDataAsync(string ipAddress, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_baseUrl) || string.IsNullOrEmpty(_apiKey))
        {
            throw new InvalidOperationException("IPStack configuration is missing.");
        }
        
        try
        {
            var client = httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{_baseUrl}/{ipAddress}?access_key={_apiKey}",
                cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var deserialized = JsonSerializer.Deserialize<IpDetails>(content);
                if (deserialized?.Ip != null)
                    return deserialized;
                else
                {
                    errorHandler.HandleApiError(response.StatusCode, content, ipAddress);
                }
            }

            errorHandler.HandleApiError(response.StatusCode, content, ipAddress);
            return null;
        }
        catch (HttpRequestException ex)
        {
            Log.Error(ex, "An error occurred while calling the IPStack API for IP: {IPAddress}", ipAddress);
            throw new ExternalServiceException("Failed to retrieve data from IPStack API.", ex);
        }
        catch (Exception ex)
        {
           Log.Error(ex, "An unexpected error occurred for IP: {IPAddress}", ipAddress);
            throw;
        }
    }
}
