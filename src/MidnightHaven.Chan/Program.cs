using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace MidnightHaven.Chan;

public static class Program
{
    private static async Task Main(string[] args)
    {
        using var host = CreateHostBuilder(args).Build();
        await host.RunAsync();
    }

    private static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration(builder => builder.AddEnvironmentVariables())
        .ConfigureServices(Startup.ConfigureServices)
        .UseSerilog((_, config) => config
            .Enrich.FromLogContext()
            .WriteTo.Console());
}
