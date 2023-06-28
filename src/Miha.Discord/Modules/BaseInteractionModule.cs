using Discord;
using Discord.Interactions;
using FluentResults;
using Miha.Discord.Extensions;

namespace Miha.Discord.Modules;

public class BaseInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    protected virtual Task RespondMinimalAsync(
        string? description,
        params EmbedFieldBuilder[] fields)
    {
        var embed = new EmbedBuilder().AsMinimal(
            Context.User.Username,
            Context.User.GetAvatarUrl(),
            description,
            fields);

        return RespondAsync(embed: embed.Build(), ephemeral: true);
    }

    protected virtual Task RespondSuccessAsync(
        string description,
        params EmbedFieldBuilder[] fields)
    {
        var embed = new EmbedBuilder().AsSuccess(
            Context.User.Username,
            Context.User.GetAvatarUrl(),
            description,
            fields);

        return RespondAsync(embed: embed.Build(), ephemeral: true);
    }

    protected virtual Task RespondFailureAsync(string description)
    {
        var embed = new EmbedBuilder().AsFailure(
            Context.User.Username,
            Context.User.GetAvatarUrl(),
            description);

        return RespondAsync(embed: embed.Build(), ephemeral: true);
    }

    protected virtual Task RespondErrorAsync(IEnumerable<IError> errors)
    {
        var embed = new EmbedBuilder().AsError(
            Context.User.Username,
            Context.User.GetAvatarUrl(),
            errors.ToArray());

        return RespondAsync(embed: embed.Build(), ephemeral: true);
    }

    protected virtual Task RespondErrorAsync(params IError[] errors)
    {
        var embed = new EmbedBuilder().AsError(
            Context.User.Username,
            Context.User.GetAvatarUrl(),
            errors);

        return RespondAsync(embed: embed.Build(), ephemeral: true);
    }

    protected virtual Task ModifyOriginalResponseToSuccessAsync(string description, params EmbedFieldBuilder[] fields)
    {
        var embed = new EmbedBuilder().AsSuccess(
            Context.User.Username,
            Context.User.GetAvatarUrl(),
            description,
            fields);

        return ModifyOriginalResponseAsync(properties =>
        {
            properties.Embed = embed.Build();
            properties.Components = null;
            properties.Content = null;
        });
    }

    protected virtual Task ModifyOriginalResponseToErrorAsync(IEnumerable<IError> errors)
    {
        var embed = new EmbedBuilder().AsError(
            Context.User.Username,
            Context.User.GetAvatarUrl(),
            errors.ToArray());

        return ModifyOriginalResponseAsync(properties =>
        {
            properties.Embed = embed.Build();
            properties.Components = null;
            properties.Content = string.Empty;
        });
    }
}
