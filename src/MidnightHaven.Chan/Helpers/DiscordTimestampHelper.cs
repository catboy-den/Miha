namespace MidnightHaven.Chan.Helpers;

public static class DiscordTimestampHelper
{
    public static class Style
    {
        public const string ShortTime = "t"; // 16:20
        public const string LongTime = "T"; // 16:20:30
        public const string ShortDate = "d"; // 20/04/2021
        public const string LongDate = "D"; // 20 April 2021
        public const string ShortDateTime = "f*"; // 20 April 2021 16:20
        public const string LongDateTime = "F"; // Tuesday, 20 April 2021 16:20
        public const string RelativeTime = "R"; // 2 months ago
    }

    public static string ToDiscordTimestamp(this DateTimeOffset offset) => "<t:" + offset.ToUnixTimeSeconds() + ">";
    public static string ToDiscordTimestamp(this DateTimeOffset offset, string style) => "<t:" + offset.ToUnixTimeSeconds() + ":" + style + ">";

    public static string? ToDiscordTimestamp(this DateTimeOffset? offset) => offset?.ToDiscordTimestamp();
    public static string? ToDiscordTimestamp(this DateTimeOffset? offset, string style) => offset?.ToDiscordTimestamp(style);
}
