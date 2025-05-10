using Serilog;

namespace MyTemplate.API.Extensions;

public static class LoggingConfiguration
{
    public static void AddSerilogLogging(this ConfigureHostBuilder host)
    {
        host.UseSerilog((context, config) =>
        {
            config.ReadFrom.Configuration(context.Configuration)
                  .Enrich.FromLogContext()
                  .WriteTo.Console()
                  .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day);
        });
    }
}
