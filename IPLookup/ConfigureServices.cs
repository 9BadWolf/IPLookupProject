using FluentValidation;
using IPLookup.Common;
using IPLookup.Common.Results;
using IPLookup.Service;
using IPLookup.Services;
using IPLookup.Types;
using Serilog;

namespace IPLookup;

public static class ConfigureServices
{
    public static void AddServices(this WebApplicationBuilder builder)
    {
        builder.AddSerilog();
        builder.AddSwagger();
        builder.Services.AddHttpClient();
        builder.Services.AddScoped<Lookup, Lookup>();
        builder.Services.AddScoped<IIpStackErrorHandler, IpStackErrorHandler>();
        builder.Services.AddScoped<IIpStackService, IpStackService>();
        builder.Services.AddOptionsWithValidateOnStart<IpStack>()
            .BindConfiguration(IpStack.ConfigurationSectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        builder.Services.AddValidatorsFromAssembly(typeof(ConfigureServices).Assembly);
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