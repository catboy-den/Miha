using Discord;
using Discord.Interactions;
using FluentResults;
using MidnightHaven.Chan.Helpers;

namespace MidnightHaven.Chan.Modules;

public class BaseInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    protected virtual Task RespondFailureAsync(IEnumerable<IError>? errors = null)
    {
        return RespondAsync(embed: EmbedHelper.Failure(
                description: "Updating Guild Options",
                authorName: Context.User.Username,
                authorIcon: Context.User.GetAvatarUrl(),
                errors: errors)
            .Build(), ephemeral: true);
    }

    protected virtual Task RespondSuccessAsync(string description, IEnumerable<EmbedFieldBuilder>? fields = null)
    {
        var embed = EmbedHelper.Success(
            description: description,
            authorName: Context.User.Username,
            authorIcon: Context.User.GetAvatarUrl());

        if (fields is not null)
        {
            embed.WithFields(fields);
        }

        return RespondAsync(embed: embed.Build(), ephemeral: true);
    }
}
