using Discord;

namespace Miha.Discord.Extensions;

public static class EmbedExtensions
{
    public static EmbedBuilder WithVersionFooter(this EmbedBuilder builder) =>
        builder.WithFooter("miha-" + ThisAssembly.Git.Commit);
}
