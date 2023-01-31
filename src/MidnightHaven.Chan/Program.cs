using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Exceptions;
using Serilog.Formatting.Compact;

namespace MidnightHaven.Chan;

public static class Program
{
    private static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .WriteTo.Console(new CompactJsonFormatter())
            .CreateLogger();

        try
        {
            using var host = CreateHostBuilder(args).Build();
            await host.RunAsync();
        }
        catch (Exception e)
        {
            Log.Error(e, "Host terminated unexpectedly");
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }

    private static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration(builder => builder.AddEnvironmentVariables())
        .ConfigureServices(Startup.ConfigureServices)
        .UseSerilog();
}
