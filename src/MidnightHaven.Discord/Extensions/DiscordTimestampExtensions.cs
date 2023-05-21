using Discord;

namespace MidnightHaven.Discord.Extensions;

public static class DiscordTimestampExtensions
{
    public static string ToDiscordTimestamp(this DateTimeOffset offset) => TimestampTag.FromDateTimeOffset(offset).ToString();
    public static string ToDiscordTimestamp(this DateTimeOffset offset, TimestampTagStyles style) => TimestampTag.FromDateTimeOffset(offset, style).ToString();

    public static string? ToDiscordTimestamp(this DateTimeOffset? offset) => offset?.ToDiscordTimestamp();
    public static string? ToDiscordTimestamp(this DateTimeOffset? offset, TimestampTagStyles style) => offset?.ToDiscordTimestamp(style);
}
