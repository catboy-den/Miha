using Discord;
using Discord.Interactions;
using MidnightHaven.Chan.Services.Logic.Interfaces;

namespace MidnightHaven.Chan.Modules.User;

/// <summary>
/// VRChat related interactions
/// </summary>
[Group("vrchat", "VRChat related commands")]
public class VrcModule : BaseInteractionModule
{
    [Group("user", "Manage your VRChat user profile in relation to your Discord profile")]
    public class UserModule : BaseInteractionModule
    {
        private readonly IUserService _userService;

        public UserModule(IUserService userService)
        {
            _userService = userService;
        }

        [SlashCommand("set", "Sets or updates your VRChat usr url, makes it easier for event-attendees to friend you")]
        public async Task SetAsync(
            [Summary("vrchatProfileUrl", "VRChat usr link, ex https://vrchat.com/home/user/usr_666ca92f-ca50-4c25-994c-03d72842c92b")] string vrchatProfileUrl)
        {
            var result = await _userService.UpdateVrcUserIdAsync(Context.User.Id, vrchatProfileUrl);

            if (result.IsFailed)
            {
                await RespondErrorAsync(result.Errors);
                return;
            }

            var userField = new EmbedFieldBuilder()
                .WithName("VRChat user")
                .WithValue(result.Value?.GetHyperLinkedVrcUsrUrl(Context.User.Username))
                .WithIsInline(false);

            await RespondSuccessAsync("Set VRChat user", userField);
        }

        [SlashCommand("get", "Get your linked VRChat usr Id & usr url")]
        public async Task GetAsync()
        {
            var result = await _userService.GetAsync(Context.User.Id);

            if (result.IsFailed)
            {
                await RespondErrorAsync(result.Errors);
                return;
            }

            if (result.Value?.VrcUsrId is null)
            {
                await RespondFailureAsync("User hasn't linked their VRChat profile");
                return;
            }

            var userId = new EmbedFieldBuilder()
                .WithName("User Id")
                .WithValue(result.Value?.VrcUsrId)
                .WithIsInline(false);

            var userUrl = new EmbedFieldBuilder()
                .WithName("User Url")
                .WithValue(result.Value?.GetHyperLinkedVrcUsrUrl())
                .WithIsInline(false);

            await RespondMinimalAsync(null, userId, userUrl);
        }

        [SlashCommand("clear", "Clears set VRChat user")]
        public async Task ClearAsync()
        {
            var result = await _userService.UpsertAsync(Context.User.Id, doc => doc.VrcUsrId = null);

            if (result.IsFailed)
            {
                await RespondErrorAsync(result.Errors);
                return;
            }

            await RespondSuccessAsync("Cleared VRChat user");
        }
    }
}
