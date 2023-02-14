using Discord;
using Discord.Interactions;
using FluentResults;
using MidnightHaven.Chan.Helpers;

namespace MidnightHaven.Chan.Modules;

public class BaseInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    protected virtual Task RespondBasicAsync(
        EmbedFieldBuilder field,
        string? title = null,
        string? description = null,
        Color? color = null)
        => RespondBasicAsync(title, description, color, new[] { field });

    protected virtual Task RespondBasicAsync(
        string? title = null,
        string? description = null,
        Color? color = null,
        IEnumerable<EmbedFieldBuilder>? fields = null)
    {
        var embed = EmbedHelper.Basic(
            title: title,
            description: description,
            authorName: Context.User.Username,
            authorIcon: Context.User.GetAvatarUrl(),
            color: color,
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
        => RespondFailureAsync(null, new List<IError> { error });

    protected virtual Task RespondFailureAsync(IEnumerable<IError>? errors = null)
        => RespondFailureAsync(null, errors);

    protected virtual Task RespondFailureAsync(string? description, IError error)
        => RespondFailureAsync(description, new List<IError> { error });

    protected virtual Task RespondFailureAsync(string? description, IEnumerable<IError>? errors = null)
    {
        return RespondAsync(embed: EmbedHelper.Failure(
                description: description,
                authorName: Context.User.Username,
                authorIcon: Context.User.GetAvatarUrl(),
                errors: errors)
            .Build(), ephemeral: true);
    }
}
