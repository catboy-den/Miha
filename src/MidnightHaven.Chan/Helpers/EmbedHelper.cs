using Discord;
using FluentResults;

namespace MidnightHaven.Chan.Helpers;

public static class EmbedHelper
{
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
            .WithFooter("v1.06")
            .WithCurrentTimestamp();
    }

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
            .WithFooter("v1.06")
            .WithCurrentTimestamp();
    }

}
