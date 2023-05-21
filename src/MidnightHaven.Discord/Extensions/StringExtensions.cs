using System.Globalization;

namespace MidnightHaven.Discord.Extensions;

public static class StringExtensions
{
    public static string ToTitleCase(this string self, string locale = "en-US") => new CultureInfo(locale).TextInfo.ToTitleCase(self);
}
