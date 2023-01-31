using Cronos;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MidnightHaven.Chan.Services.Background;

public class GuildEventStartingSoonService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<GuildEventStartingSoonService> _logger;

    private const string Schedule = "0 0/10 0 ? * * *"; // every ten minutes, starting at 00
    private readonly CronExpression _cron;
    private bool _botServiceReadied = false;

    public GuildEventStartingSoonService(
        IServiceProvider serviceProvider,
        ILogger<GuildEventStartingSoonService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        _cron = CronExpression.Parse(Schedule);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (!_botServiceReadied)
            {
                var botService = _serviceProvider
                    .GetServices<IHostedService>()
                    .OfType<BotService>()
                    .Single();

                _botServiceReadied = botService.IsClientReady();

                // Wait for 30 seconds until the bot service has indicated client-ready
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                continue;
            }

            var utcNow = DateTime.UtcNow;
            var nextUtc = _cron.GetNextOccurrence(utcNow);

            if (nextUtc is null)
            {
                _logger.LogWarning("Next utc occurence is null");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                continue;
            }

            await CheckGuildEventsAsync();
            await Task.Delay(nextUtc.Value - utcNow, stoppingToken);
        }
    }

    private async Task CheckGuildEventsAsync()
    {
        var client = _serviceProvider.GetService<DiscordSocketClient>();
    }
}
