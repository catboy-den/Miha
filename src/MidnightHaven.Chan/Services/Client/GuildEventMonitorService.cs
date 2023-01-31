using Cronos;
using Discord;
using Discord.Addons.Hosting;
using Discord.Addons.Hosting.Util;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MidnightHaven.Chan.Models.Options;

namespace MidnightHaven.Chan.Services.Client;

public class GuildEventMonitorService : DiscordClientService
{
    private readonly DiscordOptions _discordOptions;
    private readonly ILogger<GuildEventMonitorService> _logger;

    private const string Schedule = "0,10,20,30,40,50 ? * * *"; // At 0, 10, 20, 30, 40, and 50 minutes past the hour, at 12:00 AM, https://crontab.cronhub.io/

    private readonly CronExpression _cron;

    public GuildEventMonitorService(
        DiscordSocketClient client,
        IOptions<DiscordOptions> discordOptions,
        ILogger<GuildEventMonitorService> logger) : base(client, logger)
    {
        _discordOptions = discordOptions.Value;
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
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                continue;
            }

            await CheckScheduledEventsAsync();
            await Task.Delay(nextUtc.Value - utcNow, stoppingToken);
        }
    }

    private async Task CheckScheduledEventsAsync()
    {
        var guild = Client.GetGuild(_discordOptions.Guild!.Value);

        if (guild is null)
        {
            _logger.LogCritical("Guild is null {GuildId}", _discordOptions.Guild.Value);
            return;
        }

        foreach (var guildEvent in guild.Events)
        {
            var startsIn = guildEvent.StartTime.DateTime - DateTime.UtcNow;

            // "Round" our minutes up
            if (startsIn.Seconds <= 60)
            {
                startsIn = TimeSpan.FromMinutes(startsIn.Minutes + 1);
            }

            if (startsIn.Minutes is < 5 or > 20)
            {
                continue;
            }


            // Use an IMemoryCache for caching events we've already announced as starting soon
            // Send an embed instead
            // Go grab the guild options to get the announcement role & channel

            var channel = await Client.GetChannelAsync(1069326486470938736) as ITextChannel;
            await channel!.SendMessageAsync($"Event {guildEvent.Id} starts in {startsIn.Minutes} minutes");
        }
    }
}
