namespace Miha.Discord;

public class DiscordOptions
{
    public const string Section = "Discord";

    public string Token { get; set; } = "";
    public ulong? Guild { get; set; } = null;
}
