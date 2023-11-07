using Discord;
using FluentResults;
using NodaTime;

namespace Miha.Discord.Services.Interfaces;

public interface IGuildScheduledEventService
{
    Task<Result<IEnumerable<IGuildScheduledEvent>>> GetScheduledWeeklyEventsAsync(ulong guildId, LocalDate dateOfTheWeek);
}
