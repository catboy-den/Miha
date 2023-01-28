using Discord;

namespace MidnightHaven.Chan.Helpers;

public static class EmbedFieldHelper
{
    public static EmbedFieldBuilder TextChannel(string fieldName, string? channelName)
    {
        return new EmbedFieldBuilder()
            .WithName(fieldName)
            .WithValue(channelName != null ? "`#" + channelName + "`" : "`null`");
    }
}
