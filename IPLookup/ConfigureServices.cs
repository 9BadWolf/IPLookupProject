using FluentValidation;
using IPLookup.Common;
using IPLookup.Common.Results;
using IPLookup.Services;

namespace IPLookup;

public static class ConfigureServices
{
    public static void AddServices(this WebApplicationBuilder builder)
    {
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
}