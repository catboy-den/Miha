using Discord;

namespace Miha.Discord.Extensions;

public static class EmbedExtensions
{
    public static EmbedBuilder WithVersionFooter(this EmbedBuilder builder) =>
        builder.WithFooter("v" + ThisAssembly.Git.Commit);
}
