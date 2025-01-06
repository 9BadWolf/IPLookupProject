using System.Text.Json;
using IPLookup.Common;
using IPLookup.Types;
using Microsoft.Extensions.Caching.Memory;
using Serilog;

namespace CachingApi.Services;

public class CacheService
{
    private readonly IMemoryCache _cache;
    private readonly IHttpClientFactory _httpClientFactory;
    
    public CacheService(IMemoryCache cache, IHttpClientFactory httpClientFactory)
    {
        _cache = cache;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IResult> GetOrAddCacheAsync(string key)
    {
        if (_cache.TryGetValue(key, out LocationData? cachedValue))
        {
            return Results.Ok(cachedValue);
        }

        try
        {
            var client = _httpClientFactory.CreateClient("IPLookupClient");
            var response = await client.GetAsync($"http://localhost:5290/get-ip-details/{key}");

            if (!response.IsSuccessStatusCode)
                return Results.NotFound("Ip details not found in cache or external service.");
            
            var content = await response.Content.ReadAsStringAsync();
            var item = JsonSerializer.Deserialize<LocationData>(content);
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(1));
            _cache.Set(key, item, cacheEntryOptions);

            return Results.Ok(item);
        }
        catch (HttpRequestException ex)
        {
            Log.Error(ex, "An error occurred ");
            throw new ExternalServiceException("Failed to retrieve data from API.", ex);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An unexpected error occurred for IP: {IPAddress}", key);
            throw;
        }
    }
}