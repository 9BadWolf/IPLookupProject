using BatchProcessor.Common;
using BatchProcessor.Services;
using Serilog;

namespace BatchProcessor;

public static class ConfigureServices
{
    public static void AddServices(this WebApplicationBuilder builder)
    {
        builder.AddSerilog();
        builder.AddSwagger();
        builder.Services.AddHttpClient();
        builder.Services.AddSingleton<BatchJobProcessing>();
        builder.Services.AddOptionsWithValidateOnStart<CachingApi>()
            .BindConfiguration(CachingApi.ConfigurationSectionName)
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