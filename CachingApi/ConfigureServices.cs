using Cache.Common;
using CachingApi.Services;
using Serilog;

namespace Cache;

public static class ConfigureServices
{
    public static void AddServices(this WebApplicationBuilder builder)
    {
        builder.AddSerilog();
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

    private static void AddSerilog(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, configuration) =>
        {
            configuration.ReadFrom.Configuration(context.Configuration);
        });
    }
}