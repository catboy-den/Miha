using Discord;
using FluentResults;
using MidnightHaven.Chan.Extensions;

namespace MidnightHaven.Chan.Helpers;

public static class EmbedHelper
{
    /// <summary>
    /// Builds an embed to be sent as a "Success" indicator
    /// </summary>
    /// <param name="description"></param>
    /// <param name="authorName"></param>
    /// <param name="authorIcon"></param>
    /// <param name="title"></param>
    /// <returns></returns>
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
            .WithDescription("`" + description + "`")
            .WithColor(Color.Green)
            .WithVersionFooter()
            .WithCurrentTimestamp();
    }

    /// <summary>
    /// Builds an embed to be sent as a "Error" or "Failure" indicator
    /// </summary>
    /// <param name="description"></param>
    /// <param name="authorName"></param>
    /// <param name="authorIcon"></param>
    /// <param name="title">The title of the embed</param>
    /// <param name="errors">Optional, any FluentResults errors that will be included in the embed as fields</param>
    /// <returns></returns>
    public static EmbedBuilder Failure(
        string? description,
        string? authorName = null,
        string? authorIcon = null,
        string? title = "Failure",
        IEnumerable<IError>? errors = null)
    {
        var fields = new List<EmbedFieldBuilder>();

        if (errors != null)
        {
            fields.AddRange(errors.Select(error => new EmbedFieldBuilder().WithName("Error")
                .WithValue("`" + error.Message + "`")
                .WithIsInline(false)));
        }

        return new EmbedBuilder()
            .WithAuthor(authorName, authorIcon)
            .WithThumbnailUrl(authorIcon)
            .WithTitle(title)
            .WithDescription("`" + description + "`")
            .WithColor(Color.Red)
            .WithFields(fields)
            .WithVersionFooter()
            .WithCurrentTimestamp();
    }

    /// <summary>
    /// Builds an embed intended to be sent for logic related to Guild Scheduled Events
    /// </summary>
    /// <param name="eventVerb">eg, "Event cancelled" or "Event create"</param>
    /// <param name="eventName">The name of the event</param>
    /// <param name="eventLocation">The location of the event</param>
    /// <param name="eventDescription">The description of the event</param>
    /// <param name="embedColor">The color of the embed</param>
    /// <param name="authorAvatarUrl">The image or avatar url to place in the author field</param>
    /// <param name="authorUsername">Optional, the username of the event creator</param>
    /// <param name="fields">Optional, any fields to add to the embed</param>
    /// <returns></returns>
    public static EmbedBuilder ScheduledEvent(
        string eventVerb,
        string eventName,
        string eventLocation,
        string eventDescription,
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
