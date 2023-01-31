using Cronos;
using Discord.Addons.Hosting;
using Discord.Addons.Hosting.Util;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace MidnightHaven.Chan.Services.Client;

public class GuildEventStartingSoonService : DiscordClientService
{
    private readonly ILogger<GuildEventStartingSoonService> _logger;

    private const string Schedule = "0 0/10 0 ? * * *"; // every ten minutes, starting at 00
    private readonly CronExpression _cron;

    public GuildEventStartingSoonService(
        DiscordSocketClient client,
        ILogger<GuildEventStartingSoonService> logger) : base(client, logger)
    {
        _logger = logger;

        _cron = CronExpression.Parse(Schedule);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Client.WaitForReadyAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {


            var utcNow = DateTime.UtcNow;
            var nextUtc = _cron.GetNextOccurrence(utcNow);

            if (nextUtc is null)
            {
                _logger.LogWarning("Next utc occurence is null");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                continue;
            }

            await Task.Delay(nextUtc.Value - utcNow, stoppingToken);
        }
    }
}
