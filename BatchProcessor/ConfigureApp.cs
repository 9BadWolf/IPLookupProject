using System.Collections.Concurrent;
using BatchProcessor.Types;
using Hangfire;
using Serilog;
using StackExchange.Redis;

namespace BatchProcessor;

public static class ConfigureApp
{
    public static async Task Configure(this WebApplication app)
    {
        app.UseSerilogRequestLogging();
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseHttpsRedirection();
        app.MapEndpoints();
        app.UseHangfireDashboard();
        var redis = app.Services.GetRequiredService<IConnectionMultiplexer>();
        var db = redis.GetDatabase();
    }
}