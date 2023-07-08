using Discord;
using Miha.Shared;

namespace Miha.Discord.Extensions;

public static class EmbedExtensions
{
    public static EmbedBuilder WithVersionFooter(this EmbedBuilder builder) =>
        builder.WithFooter(Versioning.GetVersion());
}
