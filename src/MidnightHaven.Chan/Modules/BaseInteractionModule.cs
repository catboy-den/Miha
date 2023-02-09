using Discord;
using Discord.Interactions;
using FluentResults;
using MidnightHaven.Chan.Helpers;

namespace MidnightHaven.Chan.Modules;

public class BaseInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    protected virtual Task RespondBasicAsync(
        EmbedFieldBuilder field,
        Color? color = null,
        string? title = null)
    {
        return RespondBasicAsync(color, title, new List<EmbedFieldBuilder> { field });
    }

    protected virtual Task RespondBasicAsync(
        Color? color = null,
        string? title = null,
        IEnumerable<EmbedFieldBuilder>? fields = null)
    {
        var embed = EmbedHelper.Basic(
            authorName: Context.User.Username,
            authorIcon: Context.User.GetAvatarUrl(),
            color: color,
            title: title,
            fields: fields);

        return RespondAsync(embed: embed.Build(), ephemeral: true);
    }

    protected virtual Task RespondSuccessAsync(string description, EmbedFieldBuilder field)
    {
        return RespondSuccessAsync(description, new List<EmbedFieldBuilder> { field });
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

    protected virtual Task RespondFailureAsync(IError error)
    {
        return RespondAsync(embed: EmbedHelper.Failure(
                description: "Updating Guild Options",
                authorName: Context.User.Username,
                authorIcon: Context.User.GetAvatarUrl(),
                errors: new List<IError> { error })
            .Build(), ephemeral: true);
    }

    protected virtual Task RespondFailureAsync(IEnumerable<IError>? errors = null)
    {
        return RespondAsync(embed: EmbedHelper.Failure(
                description: "Updating Guild Options",
                authorName: Context.User.Username,
                authorIcon: Context.User.GetAvatarUrl(),
                errors: errors)
            .Build(), ephemeral: true);
    }
}
