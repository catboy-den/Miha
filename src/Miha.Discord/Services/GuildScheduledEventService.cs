using Discord;
using Discord.WebSocket;
using FluentResults;
using Microsoft.Extensions.Logging;
using Miha.Discord.Services.Interfaces;
using Miha.Shared.ZonedClocks.Interfaces;
using NodaTime;
using NodaTime.Calendars;

namespace Miha.Discord.Services;

public class GuildScheduledEventService : IGuildScheduledEventService
{
    private readonly DiscordSocketClient _discordClient;
    private readonly IEasternStandardZonedClock _easternStandardZonedClock;
    private readonly ILogger<GuildScheduledEventService> _logger;

    public GuildScheduledEventService(
        DiscordSocketClient discordClient,
        IEasternStandardZonedClock easternStandardZonedClock,
        ILogger<GuildScheduledEventService> logger)
    {
        _discordClient = discordClient;
        _easternStandardZonedClock = easternStandardZonedClock;
        _logger = logger;
    }
    
    public async Task<Result<IEnumerable<IGuildScheduledEvent>>> GetScheduledWeeklyEventsAsync(ulong guildId, LocalDate dateOfTheWeek)
    {
        var weekNumberInYear = WeekYearRules.Iso.GetWeekOfWeekYear(dateOfTheWeek);

        var guild = _discordClient.GetGuild(guildId);

        if (guild is null)
        {
            return Result.Fail<IEnumerable<IGuildScheduledEvent>>("Failed to fetch discord guild");
        }
        
        var events = await guild.GetEventsAsync();

        var eventsThisWeek = events.Where(guildEvent =>
        {
            var estDate = _easternStandardZonedClock.ToZonedDateTime(guildEvent.StartTime).Date;
            
            var weekOfDate = WeekYearRules.Iso.GetWeekOfWeekYear(estDate);

            return weekOfDate == weekNumberInYear;
        }).Cast<IGuildScheduledEvent>();

        return Result.Ok(eventsThisWeek);
    }
}
