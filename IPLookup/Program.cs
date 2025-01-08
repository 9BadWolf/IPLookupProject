using IPLookup;

try
{
    var builder = WebApplication.CreateBuilder(args);
    var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
    var logger = loggerFactory.CreateLogger<Program>();
    logger.LogInformation("Starting IPLookup");
    builder.AddServices();
    var app = builder.Build();
    await app.Configure();

    app.Run();
}
catch (Exception ex)
{
    var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<Program>();
    logger.LogError(ex, "Application terminated unexpectedly");
}