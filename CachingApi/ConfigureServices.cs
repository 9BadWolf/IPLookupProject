using Cache.Common;
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
        builder.AddConfig();        
        builder.AddCorsServices();
        builder.Services.AddHttpClient();
        builder.Services.AddMemoryCache();
        builder.Services.AddScoped<CacheService>();
        builder.Services.Configure<LookupApi>(builder.Configuration.GetSection("LookupApi"));
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
    
    private static void AddCorsServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });
    }
    
    private static void AddConfig(this WebApplicationBuilder builder)
    {
        builder.Configuration
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true) 
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true) 
            .AddEnvironmentVariables();
    }
}