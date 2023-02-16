using Discord;
using Discord.Interactions;
using FluentResults;
using MidnightHaven.Chan.Helpers;

namespace MidnightHaven.Chan.Modules;

public class BaseInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    #region Respond

    protected virtual Task RespondMinimalAsync(
        EmbedFieldBuilder field,
        string? title = null,
        string? description = null,
        Color? color = null)
    {
        return RespondMinimalAsync(title, description, color, new[] { field });
    }

    protected virtual Task RespondMinimalAsync(
        string? title = null,
        string? description = null,
        Color? color = null,
        IEnumerable<EmbedFieldBuilder>? fields = null)
    {
        return RespondAsync(embed: BuildMinimalEmbed(title, description, color, fields), ephemeral: true);
    }

    protected virtual Task RespondBasicAsync(
        EmbedFieldBuilder field,
        string? title = null,
        string? description = null,
        Color? color = null)
    {
        return RespondBasicAsync(title, description, color, new[] { field });
    }

    protected virtual Task RespondBasicAsync(
        string? title = null,
        string? description = null,
        Color? color = null,
        IEnumerable<EmbedFieldBuilder>? fields = null)
    {
        return RespondAsync(embed: BuildBasicEmbed(title, description, color, fields), ephemeral: true);
    }

    protected virtual Task RespondSuccessAsync(string description, EmbedFieldBuilder field) => RespondSuccessAsync(description, new List<EmbedFieldBuilder> { field });
    protected virtual Task RespondSuccessAsync(string description, IEnumerable<EmbedFieldBuilder>? fields = null) => RespondAsync(embed: BuildSuccessEmbed(description, fields), ephemeral: true);

    protected virtual Task RespondFailureAsync(IError error) => RespondFailureAsync(null, new List<IError> { error });
    protected virtual Task RespondFailureAsync(IEnumerable<IError>? errors = null) => RespondFailureAsync(null, errors);
    protected virtual Task RespondFailureAsync(string? description, IError error) => RespondFailureAsync(description, new List<IError> { error });
    protected virtual Task RespondFailureAsync(string? description, IEnumerable<IError>? errors = null) => RespondAsync(embed: BuildFailureEmbed(description, errors), ephemeral: true);

    #endregion

    #region ModifyOriginalResponse

    protected virtual Task ModifyOriginalResponseToBasicAsync(
        EmbedFieldBuilder field,
        string? title = null,
        string? description = null,
        Color? color = null)
    {
        return ModifyOriginalResponseToBasicAsync(title, description, color, new[] { field });
    }

    protected virtual Task ModifyOriginalResponseToBasicAsync(
        string? title = null,
        string? description = null,
        Color? color = null,
        IEnumerable<EmbedFieldBuilder>? fields = null)
    {
        return ModifyOriginalResponseAsync(properties =>
        {
            properties.Embed = BuildBasicEmbed(title, description, color, fields);
            properties.Components = null;
            properties.Content = null;
        });
    }

    protected virtual Task ModifyOriginalResponseToSuccessAsync(string description, EmbedFieldBuilder field) => ModifyOriginalResponseToSuccessAsync(description, new List<EmbedFieldBuilder> { field });
    protected virtual Task ModifyOriginalResponseToSuccessAsync(string description, IEnumerable<EmbedFieldBuilder>? fields = null)
    {
        return ModifyOriginalResponseAsync(properties =>
        {
            properties.Embed = BuildSuccessEmbed(description, fields);
            properties.Components = null;
            properties.Content = null;
        });
    }

    protected virtual Task ModifyOriginalResponseToFailureAsync(IError error) => ModifyOriginalResponseToFailureAsync(null, new List<IError> { error });
    protected virtual Task ModifyOriginalResponseToFailureAsync(IEnumerable<IError>? errors = null) => ModifyOriginalResponseToFailureAsync(null, errors);
    protected virtual Task ModifyOriginalResponseToFailureAsync(string? description, IError error) => ModifyOriginalResponseToFailureAsync(description, new List<IError> { error });
    protected virtual Task ModifyOriginalResponseToFailureAsync(string? description, IEnumerable<IError>? errors = null)
    {
        return ModifyOriginalResponseAsync(properties =>
        {
            properties.Embed = BuildFailureEmbed(description: description, errors: errors);
            properties.Components = null;
            properties.Content = string.Empty;
        });
    }

    private Embed BuildMinimalEmbed(
        string? title = null,
        string? description = null,
        Color? color = null,
        IEnumerable<EmbedFieldBuilder>? fields = null)
    {
        return EmbedHelper.Basic(
            title: title,
            description: description,
            color: color,
            fields: fields).Build();
    }

    private Embed BuildBasicEmbed(
        string? title = null,
        string? description = null,
        Color? color = null,
        IEnumerable<EmbedFieldBuilder>? fields = null)
    {
        return EmbedHelper.Basic(
            title: title,
            description: description,
            authorName: Context.User.Username,
            authorIcon: Context.User.GetAvatarUrl(),
            color: color,
            fields: fields).Build();
    }

    private Embed BuildSuccessEmbed(string description, IEnumerable<EmbedFieldBuilder>? fields = null)
    {
        var embed = EmbedHelper.Success(
            description: description,
            authorName: Context.User.Username,
            authorIcon: Context.User.GetAvatarUrl());

        if (fields is not null)
        {
            embed.WithFields(fields);
        }

        return embed.Build();
    }

    private Embed BuildFailureEmbed(string? description, IEnumerable<IError>? errors = null)
    {
        return EmbedHelper.Failure(
            description: description,
            authorName: Context.User.Username,
            authorIcon: Context.User.GetAvatarUrl(),
            errors: errors).Build();
    }

    #endregion
}
