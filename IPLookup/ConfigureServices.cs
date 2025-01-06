using FluentValidation;
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
        builder.AddIpStackClient();
        builder.AddCorsServices();
        builder.Services.AddScoped<Lookup, Lookup>();
        builder.Services.AddScoped<IIpStackErrorHandler, IpStackErrorHandler>();
        builder.Services.AddScoped<IIpStackService, IpStackService>();
        builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));
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

    private static void AddIpStackClient(this WebApplicationBuilder builder)
    {
        builder.Services.AddHttpClient("IPstack", client =>
        {
            client.BaseAddress = new Uri("https://api.ipstack.com/");
           // client.DefaultRequestHeaders.Add("Accept", "application/json");
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
}