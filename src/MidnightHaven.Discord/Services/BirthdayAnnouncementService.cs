using Cronos;
using Discord.Addons.Hosting;
using Discord.Addons.Hosting.Util;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using MidnightHaven.Logic.Services.Interfaces;

namespace MidnightHaven.Discord.Services;

public class BirthdayAnnouncementService : DiscordClientService
{
    private readonly DiscordSocketClient _client;
    private readonly ILogger<BirthdayAnnouncementService> _logger;
    private const string Schedule = "0,5,10,15,20,25,30,35,40,45,50,55 8-19 * * *"; // https://crontab.cronhub.io/

    private readonly CronExpression _cron;

    public BirthdayAnnouncementService(
        DiscordSocketClient client,
        IBirthdayJobService birthdayJobService,
        ILogger<BirthdayAnnouncementService> logger) : base(client, logger)
    {
        _client = client;
        _logger = logger;

        _cron = CronExpression.Parse(Schedule, CronFormat.Standard);
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
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                continue;
            }

            await Task.Delay(nextUtc.Value - utcNow, stoppingToken);
        }
    }
}
