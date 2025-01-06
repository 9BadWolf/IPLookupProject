using BatchProcessor.Services;
using Hangfire;
using Hangfire.Redis.StackExchange;
using Serilog;
using StackExchange.Redis;

namespace BatchProcessor;

public static class ConfigureServices
{
    public static void AddServices(this WebApplicationBuilder builder)
    {
        builder.AddSerilog();
        builder.AddSwagger();
        builder.AddHangfireRedis();
        builder.Services.AddSingleton<BatchService>();
        builder.Services.AddSingleton<RedisService>();
        builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(builder.Configuration.GetValue<string>("Redis:ConnectionString")));
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

    private static void AddHangfireRedis(this WebApplicationBuilder builder)
    {
        var redisConnectionString = builder.Configuration.GetValue<string>("Redis:ConnectionString");
        builder.Services.AddHangfire(x => x.UseRedisStorage(redisConnectionString));
         builder.Services.AddHangfireServer();
        
    }
}