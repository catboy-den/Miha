using System.Text.RegularExpressions;
using Discord.Interactions;

namespace MidnightHaven.Chan.Modules;

/// <summary>
/// Registration module for linking vrchat profiles to users
/// </summary>
public partial class LinkModule : BaseInteractionModule
{
    [SlashCommand("link", "Link your VRChat profile, to your Discord profile, this only makes it easier for event-attendees to find you")]
    public async Task LinkAsync(
        [Summary("vrchatProfileUrl", "The VRChat link to your VRChat profile, found on the website")] string vrchatProfileUrl)
    {
        var usrId = UsrRegex().Match(vrchatProfileUrl);



        await RespondSuccessAsync("Linked VRChat profile");
    }

    [GeneratedRegex("usr_[a-fA-F0-9]{8}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{12}")]
    private static partial Regex UsrRegex();
}
