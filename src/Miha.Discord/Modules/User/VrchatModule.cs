using Discord;
using Discord.Interactions;
using Miha.Discord.Extensions;
using Miha.Logic.Services.Interfaces;

namespace Miha.Discord.Modules.User;

/// <summary>
/// VRChat related interactions
/// </summary>
[Group("vrchat", "Manage your VRChat user profile in relation to your Discord profile")]
public class VrchatModule : BaseInteractionModule
{
    private readonly IUserService _userService;

    public VrchatModule(IUserService userService)
    {
        _userService = userService;
    }

    [SlashCommand("get", "Gets your, or another users, VRChat profile information")]
    public async Task GetAsync(IUser? user = null)
    {
        var targetUser = user ?? Context.User;
        var result = await _userService.GetAsync(targetUser.Id);
        var userDoc = result.Value;

        if (result.IsFailed)
        {
            await RespondErrorAsync(result.Errors);
            return;
        }

        var embed = new EmbedBuilder().AsMinimal(
            targetUser.Username,
            targetUser.GetAvatarUrl(),
            null);

        if (userDoc?.VrcUserId is null)
        {
            embed.Description = targetUser.Mention + " hasn't linked their VRChat profile";
            await RespondAsync(embed: embed.Build(), ephemeral: true);
            return;
        }

        embed
            .WithThumbnailUrl(targetUser.GetAvatarUrl())
            .WithFields(new EmbedFieldBuilder()
                .WithName("VRChat profile")
                .WithValue(userDoc.GetHyperLinkedVrcUsrUrl(targetUser.Username)));

        await RespondAsync(embed: embed.Build(), ephemeral: true);
    }

    [SlashCommand("set", "Sets or updates your VRChat user profile, makes it easier for event-attendees to find you")]
    public async Task SetAsync(
        [Summary("vrchatProfileUrl", "VRChat user link, ex https://vrchat.com/home/user/usr_666ca92f-ca50-4c25-994c-03d72842c92b")] string vrchatProfileUrl)
    {
        var result = await _userService.UpsertVrchatUserIdAsync(Context.User.Id, vrchatProfileUrl);

        if (result.IsFailed)
        {
            await RespondErrorAsync(result.Errors);
            return;
        }

        await RespondSuccessAsync("VRChat profile updated");
    }

    [SlashCommand("clear", "Clears your VRChat profile")]
    public async Task ClearAsync()
    {
        var result = await _userService.UpsertAsync(Context.User.Id, doc => doc.VrcUserId = null);

        if (result.IsFailed)
        {
            await RespondErrorAsync(result.Errors);
            return;
        }

        await RespondSuccessAsync("Cleared your VRChat profile");
    }
}
