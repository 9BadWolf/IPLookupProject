using Cache.Common;
using CachingApi.Services;

namespace Cache;

public static class ConfigureServices
{
    public static void AddServices(this WebApplicationBuilder builder)
    {
        builder.AddSwagger();
        builder.Services.AddHttpClient();
        builder.Services.AddMemoryCache();
        builder.Services.AddScoped<CacheService>();
        builder.Services.AddOptionsWithValidateOnStart<LookupApi>()
            .BindConfiguration(LookupApi.ConfigurationSectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }

    private static void AddSwagger(this WebApplicationBuilder builder)
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
    }
}