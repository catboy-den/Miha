using Discord;

namespace MidnightHaven.Chan.Extensions;

public static class EmbedExtensions
{
    public static EmbedBuilder WithVersionFooter(this EmbedBuilder builder) =>
        builder.WithFooter("v"+ ThisAssembly.AssemblyInformationalVersion);
}
