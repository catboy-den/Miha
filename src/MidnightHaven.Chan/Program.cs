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
        .ConfigureServices(Startup.ConfigureServices)
        .UseSerilog((_, config) =>
        {
            config.Enrich.FromLogContext();
            config.WriteTo.Console();
        });
}
