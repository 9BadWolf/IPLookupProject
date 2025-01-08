namespace BatchProcessor;

public static class ConfigureApp
{
    public static async Task Configure(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseHttpsRedirection();
        app.MapEndpoints();
    }
}