using Discord.Interactions;
using FluentResults;
using MidnightHaven.Chan.Helpers;

namespace MidnightHaven.Chan.Modules;

public class BaseInteractionModule : InteractionModuleBase<SocketInteractionContext>
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
