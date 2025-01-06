using CachingApi.Services;
using Microsoft.AspNetCore.Builder;
using Serilog;

namespace Cache;

public static class ConfigureServices
{
    public static void AddServices(this WebApplicationBuilder builder)
    {
        builder.AddSerilog();
        builder.AddSwagger();
        builder.AddIPLookupClient();
        builder.Services.AddMemoryCache();
        builder.Services.AddScoped<CacheService>();
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
    
    private static void AddIPLookupClient(this WebApplicationBuilder builder)
    {
        builder.Services.AddHttpClient("IPLookup", client =>
        {
            client.BaseAddress = new Uri("http://localhost:5290");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });
    }
}