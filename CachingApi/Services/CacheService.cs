using System.Text.Json;
using Cache.Common;
using IPLookup.Common;
using IPLookup.Types;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Serilog;

namespace CachingApi.Services;

public class CacheService
{
    private readonly IMemoryCache _cache;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly LookupApi _options;
    
    public CacheService(IMemoryCache cache, IHttpClientFactory httpClientFactory, IOptions<LookupApi> options)
    {
        _cache = cache;
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
    }
    
    public async Task<Results<Ok<IpDetails>,NotFound<string>>> GetOrAddCacheAsync(string ipAddress)
    {
        if (_cache.TryGetValue(ipAddress, out IpDetails? cachedValue))
        {
            return TypedResults.Ok(cachedValue);
        }

        try
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{_options.BaseUrl}ipdetails/{ipAddress}");

            if (!response.IsSuccessStatusCode)
                return TypedResults.NotFound("Ip details not found in cache or external service.");
            
            var content = await response.Content.ReadAsStringAsync();
            var item = JsonSerializer.Deserialize<IpDetails>(content);
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(1));
            _cache.Set(ipAddress, item, cacheEntryOptions);

            return TypedResults.Ok(item);
        }
        catch (HttpRequestException ex)
        {
            Log.Error(ex, "An error occurred ");
            throw new ExternalServiceException("Failed to retrieve data from API.", ex);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An unexpected error occurred for IP: {IPAddress}", ipAddress);
            throw;
        }
    }

    public async Task<IpDetails?> AddCacheAsync(string ipAddress, IpDetails details )
    {
        Log.Information("Adding to cache.");

        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(1));
        _cache.Set(ipAddress, details, cacheEntryOptions);
        
        if (_cache.TryGetValue(ipAddress, out IpDetails? cachedItem))
        {
            return cachedItem;
        }
        
        Log.Error("Failed to add to cache.");
        
        return null;
    }
}