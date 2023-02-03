using Discord;

namespace MidnightHaven.Chan.Helpers;

public static class DiscordTimestampHelper
{
    public static string ToDiscordTimestamp(this DateTimeOffset offset) => TimestampTag.FromDateTimeOffset(offset).ToString();
    public static string ToDiscordTimestamp(this DateTimeOffset offset, TimestampTagStyles style) => TimestampTag.FromDateTimeOffset(offset, style).ToString();

    public static string? ToDiscordTimestamp(this DateTimeOffset? offset) => offset?.ToDiscordTimestamp();
    public static string? ToDiscordTimestamp(this DateTimeOffset? offset, TimestampTagStyles style) => offset?.ToDiscordTimestamp(style);
}
