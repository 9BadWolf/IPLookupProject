using System.Collections.Concurrent;
using BatchProcessor.Common;
using BatchProcessor.Services;
using BatchProcessor.Types;

namespace BatchProcessor;

public static class ConfigureServices
{
    public static void AddServices(this WebApplicationBuilder builder)
    {
        builder.AddSwagger();
        builder.Services.AddHttpClient();
        builder.Services.AddSingleton<ConcurrentQueue<Batch>>();
        builder.Services.AddSingleton<BatchJobProcessing>();
        builder.Services.AddHostedService<BatchJobProcessing>();
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
}