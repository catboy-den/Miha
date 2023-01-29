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

    public static EmbedFieldBuilder Role(string fieldName, string? roleName)
    {
        return new EmbedFieldBuilder()
            .WithName(fieldName)
            .WithValue(roleName != null ? "@" + roleName : "null");
    }
}
