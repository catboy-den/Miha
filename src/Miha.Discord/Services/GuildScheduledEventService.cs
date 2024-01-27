using Discord;
using Discord.WebSocket;
using FluentResults;
using Miha.Discord.Services.Interfaces;
using Miha.Shared.ZonedClocks.Interfaces;
using NodaTime;
using NodaTime.Calendars;

namespace Miha.Discord.Services;

public class GuildScheduledEventService(
    DiscordSocketClient discordClient,
    IEasternStandardZonedClock easternStandardZonedClock) : IGuildScheduledEventService
{
    public async Task<Result<IEnumerable<IGuildScheduledEvent>>> GetScheduledWeeklyEventsAsync(ulong guildId, LocalDate dateOfTheWeek)
    {
        var weekNumberInYear = WeekYearRules.Iso.GetWeekOfWeekYear(dateOfTheWeek);

        var guild = discordClient.GetGuild(guildId);

        if (guild is null)
        {
            return Result.Fail<IEnumerable<IGuildScheduledEvent>>("Failed to fetch discord guild");
        }
        
        var events = await guild.GetEventsAsync();

        var eventsThisWeek = events.Where(guildEvent =>
        {
            var estDate = easternStandardZonedClock.ToZonedDateTime(guildEvent.StartTime).Date;
            
            var weekOfDate = WeekYearRules.Iso.GetWeekOfWeekYear(estDate);

            return weekOfDate == weekNumberInYear;
        }).Cast<IGuildScheduledEvent>();

        return Result.Ok(eventsThisWeek);
    }
}
