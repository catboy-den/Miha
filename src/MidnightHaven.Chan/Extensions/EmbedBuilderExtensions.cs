using Discord;
using FluentResults;

namespace MidnightHaven.Chan.Extensions;

public static class EmbedBuilderExtensions
{
    public static EmbedBuilder AsMinimal(
        this EmbedBuilder _,
        string username,
        string avatarUrl,
        string? description,
        params EmbedFieldBuilder[] fields)
    {
        var builder = new EmbedBuilder();

        builder
            .WithAuthor(username, avatarUrl)
            .WithColor(Color.Purple)
            .WithVersionFooter()
            .WithCurrentTimestamp();

        if (description is not null)
        {
            builder.WithDescription(description);
        }

        if (fields.Length > 0)
        {
            builder.WithThumbnailUrl(avatarUrl);
            builder.WithFields(fields);
        }

        return builder;
    }

    public static EmbedBuilder AsSuccess(
        this EmbedBuilder _,
        string username,
        string avatarUrl,
        string description,
        params EmbedFieldBuilder[] fields)
    {
        var builder = new EmbedBuilder();

        builder
            .WithAuthor(username + " - Success", avatarUrl)
            .WithThumbnailUrl(avatarUrl)
            .WithColor(Color.Green)
            .WithDescription("```" + description + "```")
            .WithVersionFooter()
            .WithCurrentTimestamp();

        if (fields.Length > 0)
        {
            builder.WithFields(fields);
        }

        return builder;
    }

    public static EmbedBuilder AsFailure(
        this EmbedBuilder _,
        string username,
        string avatarUrl,
        string description)
    {
        return new EmbedBuilder()
            .WithAuthor(username + " - Failed", avatarUrl)
            .WithThumbnailUrl(avatarUrl)
            .WithColor(Color.Red)
            .WithDescription("```" + description + "```")
            .WithVersionFooter()
            .WithCurrentTimestamp();
    }

    public static EmbedBuilder AsError(
        this EmbedBuilder _,
        string username,
        string avatarUrl,
        params IError[] errors)
    {
        var fields = new List<EmbedFieldBuilder>();

        fields.AddRange(errors.Select(error => new EmbedFieldBuilder().WithName("Error")
            .WithValue("```" + error.Message + "```")
            .WithIsInline(false)));

        return new EmbedBuilder()
            .WithAuthor(username  + " - Error", avatarUrl)
            .WithThumbnailUrl(avatarUrl)
            .WithColor(Color.Red)
            .WithFields(fields)
            .WithVersionFooter()
            .WithCurrentTimestamp();
    }

    public static EmbedBuilder AsBirthday(this EmbedBuilder _)
    {
        return new EmbedBuilder();
    }

    public static EmbedBuilder AsScheduledEvent(
        this EmbedBuilder _,
        string eventVerb,
        string eventName,
        string eventLocation,
        string? eventDescription,
        Color color,
        string authorAvatarUrl,
        string? authorUsername = null,
        string? eventImageUrl = null,
        IEnumerable<EmbedFieldBuilder>? fields = null)
    {
        var embed = new EmbedBuilder()
            .WithTitle(eventLocation + " - " + eventName)
            .WithDescription(eventDescription)
            .WithColor(color)
            .WithVersionFooter()
            .WithCurrentTimestamp();

        if (!string.IsNullOrEmpty(eventDescription))
        {
            embed.WithDescription(eventDescription);
        }

        if (fields is not null)
        {
            embed.WithFields(fields);
        }

        if (string.IsNullOrEmpty(authorUsername))
        {
            embed.WithAuthor(eventVerb, authorAvatarUrl);
        }
        else
        {
            embed
                .WithAuthor(authorUsername + " - " + eventVerb, authorAvatarUrl)
                .WithThumbnailUrl(authorAvatarUrl);
        }

        if (!string.IsNullOrEmpty(eventImageUrl))
        {
            embed.WithImageUrl(eventImageUrl);
        }

        return embed;
    }
}
