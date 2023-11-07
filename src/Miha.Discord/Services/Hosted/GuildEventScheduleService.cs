using Cronos;
using Discord.Addons.Hosting;
using Discord.Addons.Hosting.Util;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Miha.Discord.Services.Interfaces;
using Miha.Shared.ZonedClocks.Interfaces;

namespace Miha.Discord.Services.Hosted;

public class GuildEventScheduleService : DiscordClientService
{
    private readonly DiscordSocketClient _client;
    private readonly IEasternStandardZonedClock _easternStandardZonedClock;
    private readonly IGuildScheduledEventService _scheduledEventService;
    private readonly DiscordOptions _discordOptions;
    private readonly ILogger<GuildEventScheduleService> _logger;
    private const string Schedule = "0,5,10,15,20,25,30,35,40,45,50,55 8-19 * * *"; // https://crontab.cronhub.io/

    private readonly CronExpression _cron;

    public GuildEventScheduleService(
        DiscordSocketClient client,
        IEasternStandardZonedClock easternStandardZonedClock,
        IGuildScheduledEventService scheduledEventService,
        IOptions<DiscordOptions> discordOptions,
        ILogger<GuildEventScheduleService> logger) : base(client, logger)
    {
        _client = client;
        _easternStandardZonedClock = easternStandardZonedClock;
        _scheduledEventService = scheduledEventService;
        _discordOptions = discordOptions.Value;
        _logger = logger;

        _cron = CronExpression.Parse(Schedule, CronFormat.Standard);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Client.WaitForReadyAsync(stoppingToken);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            var utcNow = _easternStandardZonedClock.GetCurrentInstant().ToDateTimeUtc();
            var nextUtc = _cron.GetNextOccurrence(DateTimeOffset.UtcNow, _easternStandardZonedClock.GetTimeZoneInfo());

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
