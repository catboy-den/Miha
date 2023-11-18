using Discord;
using NodaTime;

namespace Miha.Discord.Extensions;

public static class DiscordTimestampExtensions
{
    public static string ToDiscordTimestamp(this DateTimeOffset offset) => TimestampTag.FromDateTimeOffset(offset).ToString();
    public static string ToDiscordTimestamp(this DateTimeOffset offset, TimestampTagStyles style) => TimestampTag.FromDateTimeOffset(offset, style).ToString();

    public static string ToDiscordTimestamp(this Instant instant) => TimestampTag.FromDateTimeOffset(instant.ToDateTimeOffset()).ToString();
    public static string ToDiscordTimestamp(this Instant instant, TimestampTagStyles style) => TimestampTag.FromDateTimeOffset(instant.ToDateTimeOffset(), style).ToString();
    
    public static string? ToDiscordTimestamp(this DateTimeOffset? offset) => offset?.ToDiscordTimestamp();
    public static string? ToDiscordTimestamp(this DateTimeOffset? offset, TimestampTagStyles style) => offset?.ToDiscordTimestamp(style);
}
