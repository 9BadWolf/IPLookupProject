using Serilog;

namespace CachingApi;

public static class ConfigureApp
{
    public static async Task Configure(this WebApplication app)
    {
      app.UseSerilogRequestLogging();
      app.UseSwagger();
      app.UseSwaggerUI();
     app.UseHttpsRedirection();
      app.MapEndpoints();
    }
}