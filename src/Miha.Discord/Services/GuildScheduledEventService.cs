using Discord;
using Discord.WebSocket;
using FluentResults;
using Microsoft.Extensions.Logging;
using Miha.Discord.Services.Interfaces;
using Miha.Shared;
using NodaTime;
using NodaTime.Calendars;
using NodaTime.Extensions;

namespace Miha.Discord.Services;

public class GuildScheduledEventService : IGuildScheduledEventService
{
    private readonly DiscordSocketClient _discordClient;
    private readonly ILogger<GuildScheduledEventService> _logger;

    public GuildScheduledEventService(
        DiscordSocketClient discordClient,
        ILogger<GuildScheduledEventService> logger)
    {
        _discordClient = discordClient;
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
            var estDate = guildEvent.StartTime.ToZonedDateTime()
                .WithZone(DateTimeZoneProviders.Tzdb[Timezones.IanaEasternTime]).Date;
            var weekOfDate = WeekYearRules.Iso.GetWeekOfWeekYear(estDate);

            return weekOfDate == weekNumberInYear;
        }).Cast<IGuildScheduledEvent>();

        return Result.Ok(eventsThisWeek);
    }
}
