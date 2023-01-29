using Discord.Interactions;
using FluentResults;

namespace MidnightHaven.Chan.Helpers;

public class HavenModule : InteractionModuleBase<SocketInteractionContext>
{
    protected virtual async Task RespondFailureAsync(IEnumerable<IError>? errors = null)
    {
        await RespondAsync(embed: EmbedHelper.Failure(
                description: "Updating Guild Options",
                authorName: Context.User.Username,
                authorIcon: Context.User.GetAvatarUrl(),
                errors: errors)
            .Build(), ephemeral: true);
    }
}
