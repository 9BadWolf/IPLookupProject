using System.Text.Json;
using Cache.Common;
using IPLookup.Common;
using IPLookup.Types;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace CachingApi.Services;

public class CacheService
{
    private readonly IMemoryCache _cache;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<CacheService> _logger;
    private readonly LookupApi _options;

    public CacheService(IMemoryCache cache, IHttpClientFactory httpClientFactory, IOptions<LookupApi> options,
        ILogger<CacheService> logger)
    {
        _cache = cache;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<Results<Ok<IpDetails>, NotFound<string>>> GetOrAddCacheAsync(string ipAddress)
    {
        if (_cache.TryGetValue(ipAddress, out IpDetails? cachedValue))
        {
            _logger.LogInformation("Found cached ip {IpAddress}", ipAddress);
            return TypedResults.Ok(cachedValue);
        }

        try
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{_options.BaseUrl}/ipdetails/{ipAddress}");

            if (!response.IsSuccessStatusCode)
                return TypedResults.NotFound("Ip details not found in cache or external service.");

            _logger.LogInformation("Add {IpAddress} details to cache", ipAddress);

            var content = await response.Content.ReadAsStringAsync();
            var item = JsonSerializer.Deserialize<IpDetails>(content);
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(1));
            _cache.Set(ipAddress, item, cacheEntryOptions);

            return TypedResults.Ok(item);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "An error occurred ");
            throw new ExternalServiceException("Failed to retrieve data from API.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred for IP: {IPAddress}", ipAddress);
            throw;
        }
    }

    public async Task<IpDetails?> AddCacheAsync(string ipAddress, IpDetails details)
    {
        _logger.LogInformation("Adding to cache.");

        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(1));
        _cache.Set(ipAddress, details, cacheEntryOptions);

        if (_cache.TryGetValue(ipAddress, out IpDetails? cachedItem)) return cachedItem;

        _logger.LogError("Failed to add to cache.");

        return null;
    }
}