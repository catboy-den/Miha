using Discord;
using FluentResults;
using MidnightHaven.Chan.Extensions;

namespace MidnightHaven.Chan.Helpers;

public static class EmbedHelper
{
    public static EmbedBuilder Basic(
        string? title = null,
        string? description = null,
        string? authorName = null,
        string? authorIcon = null,
        Color? color = null,
        IEnumerable<EmbedFieldBuilder>? fields = null)
    {
        var embed = new EmbedBuilder()
            .WithAuthor(authorName, authorIcon)
            .WithThumbnailUrl(authorIcon)
            .WithVersionFooter()
            .WithCurrentTimestamp();

        if (title is not null)
        {
            embed.WithTitle(title);
        }

        if (description is not null)
        {
            embed.WithDescription(description);
        }

        if (color is not null)
        {
            embed.WithColor(color.Value);
        }

        if (fields is not null)
        {
            embed.WithFields(fields);
        }

        return embed;
    }

    public static EmbedBuilder Success(
        string? description,
        string? authorName = null,
        string? authorIcon = null,
        string? title = "Success")
    {
        return new EmbedBuilder()
            .WithAuthor(authorName, authorIcon)
            .WithThumbnailUrl(authorIcon)
            .WithTitle(title)
            .WithDescription("```+\n" + description + "\n```")
            .WithColor(Color.Green)
            .WithVersionFooter()
            .WithCurrentTimestamp();
    }

    public static EmbedBuilder Failure(
        string? description = null,
        string? authorName = null,
        string? authorIcon = null,
        string? title = "Failure",
        IEnumerable<IError>? errors = null)
    {
        var builder = new EmbedBuilder();
        var fields = new List<EmbedFieldBuilder>();

        if (errors is not null)
        {
            fields.AddRange(errors.Select(error => new EmbedFieldBuilder().WithName("Error")
                .WithValue("`" + error.Message + "`")
                .WithIsInline(false)));
        }

        if (description is not null)
        {
            builder.WithDescription("```+\n" + description + "\n```");
        }

        return builder
            .WithAuthor(authorName, authorIcon)
            .WithThumbnailUrl(authorIcon)
            .WithTitle(title)
            .WithColor(Color.Red)
            .WithFields(fields)
            .WithVersionFooter()
            .WithCurrentTimestamp();
    }

    public static EmbedBuilder ScheduledEvent(
        string eventVerb,
        string eventName,
        string eventLocation,
        string? eventDescription,
        Color embedColor,
        string authorAvatarUrl,
        string? authorUsername = null,
        string? eventImageUrl = null,
        IEnumerable<EmbedFieldBuilder>? fields = null)
    {
        var embed = new EmbedBuilder()
            .WithTitle(eventLocation + " - " + eventName)
            .WithDescription(eventDescription)
            .WithColor(embedColor)
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
