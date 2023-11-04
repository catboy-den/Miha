using Cronos;
using Discord.Addons.Hosting;
using Discord.Addons.Hosting.Util;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Miha.Logic.Services.Interfaces;
using Miha.Redis.Documents;
using Miha.Shared.ZonedClocks.Interfaces;

namespace Miha.Discord.Services;

/// <summary>
/// Announces birthdays to a channel by checking and using <see cref="BirthdayJobDocument"/>s in the database
/// </summary>
public partial class BirthdayAnnouncementService : DiscordClientService
{
    private readonly DiscordSocketClient _client;
    private readonly IGuildService _guildService;
    private readonly IUserService _userService;
    private readonly IBirthdayJobService _birthdayJobService;
    private readonly IEasternStandardZonedClock _easternStandardZonedClock;
    private readonly DiscordOptions _discordOptions;
    private readonly ILogger<BirthdayAnnouncementService> _logger;
    private const string Schedule = "0,5,10,15,20,25,30,35,40,45,50,55 8-19 * * *"; // https://crontab.cronhub.io/

    private readonly CronExpression _cron;

    public BirthdayAnnouncementService(
        DiscordSocketClient client,
        IGuildService guildService,
        IUserService userService,
        IBirthdayJobService birthdayJobService,
        IEasternStandardZonedClock easternStandardZonedClock,
        IOptions<DiscordOptions> discordOptions,
        ILogger<BirthdayAnnouncementService> logger) : base(client, logger)
    {
        _client = client;
        _guildService = guildService;
        _userService = userService;
        _birthdayJobService = birthdayJobService;
        _easternStandardZonedClock = easternStandardZonedClock;
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

            await AnnounceBirthdaysAsync();
        }
    }

    private async Task AnnounceBirthdaysAsync()
    {
        SocketGuild guild;

        try
        {
            guild = Client.GetGuild(_discordOptions.Guild!.Value);
            if (guild is null)
            {
                _logger.LogCritical("Guild is null {GuildId}", _discordOptions.Guild.Value);
                return;
            }
        }
        catch (Exception e)
        {
            LogError(e);
            return;
        }

        var birthdayAnnouncementChannel = await _guildService.GetBirthdayAnnouncementChannelAsync(guild.Id);
        if (birthdayAnnouncementChannel.IsFailed)
        {
            // TODO
            return;
        }

        var jobDocuments = await _birthdayJobService.GetAllAsync();
        if (jobDocuments.IsFailed)
        {
            // TODO
            return;
        }

        var today = _easternStandardZonedClock.GetCurrentDate();
        foreach (var birthday in jobDocuments.Value.Where(s => s.BirthdayDate == today))
        {
            var user = await _userService.GetAsync(birthday.UserId);
            var userDoc = await _userService.GetAsync(birthday.UserId);

            if (user.IsFailed || user.Value is null)
            {
                continue;
            }

            if (userDoc.IsFailed || userDoc.Value is null)
            {
                continue;
            }

            if (userDoc.Value.EnableBirthday is false)
            {
                await _userService.UpsertAsync(userDoc.Value.Id, doc => doc.LastBirthdateAnnouncement = today);
            }

            // do announcement
            
            var result = await _userService.UpsertAsync(userDoc.Value.Id, doc => doc.LastBirthdateAnnouncement = today);
            var delete = await _birthdayJobService.DeleteAsync(birthday.Id);
        }


        // pull all birthday job documents, remove any that don't have a birth date of today (in est, should be already converted)
        // for each birthday job document, we need to get the userDoc and user for it
        // if the userDoc birthday is disabled, remove the birthdayjobdocument and set it as announced
        // if the user has a role that isn't whitelisted, remove the job doc and set it as announced
        // announce the birthday finally and set it as announced
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "Exception occurred in BirthdayAnnouncementService")]
    public partial void LogError(Exception e);
    
    [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "Failed to get the configured announcement channel")]
    public partial void LogBirthdayAnnouncementChannelFailure();
}
