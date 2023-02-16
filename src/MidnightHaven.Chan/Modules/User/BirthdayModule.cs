﻿using System.Text;
using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Logging;
using MidnightHaven.Chan.Extensions;
using MidnightHaven.Chan.Helpers;
using MidnightHaven.Chan.Services.Logic.Interfaces;
using NodaTime;
using NodaTime.Extensions;
using NodaTime.Text;
using TimeZoneConverter;

namespace MidnightHaven.Chan.Modules.User;

/// <summary>
/// Birthday related interactions
/// </summary>
[Group("birthday", "Birthday related commands")]
public class BirthdayModule : BaseInteractionModule
{
    private static readonly AnnualDatePattern BirthdatePattern = AnnualDatePattern.CreateWithInvariantCulture("MM/dd");

    private readonly IClock _clock;
    private readonly IUserService _userService;
    private readonly ILogger<BirthdayModule> _logger;

    public BirthdayModule(
        IClock clock,
        IUserService userService,
        ILogger<BirthdayModule> logger)
    {
        _clock = clock;
        _userService = userService;
        _logger = logger;
    }

    [SlashCommand("get", "Gets your, or another users, birthday")]
    public async Task GetAsync(IUser? user = null)
    {
        var targetUser = user ?? Context.User;

        var result = await _userService.GetAsync(targetUser.Id);

        if (result.IsFailed)
        {
            await RespondFailureAsync("Failed to get user document", result.Errors);
            return;
        }

        if (result.Value is null || result.Value.EnableBirthday is false)
        {
            var embed = new EmbedBuilder()
                .WithColor(Color.Magenta)
                .WithDescription(targetUser.Mention + " doesn't have a birthday set")
                .WithAuthor(targetUser.Username, targetUser.GetAvatarUrl())
                .WithVersionFooter()
                .WithCurrentTimestamp();

            await RespondAsync(embed: embed.Build(), ephemeral: true);
        }
    }

    [SlashCommand("set", "Sets or updates your birthday")]
    public async Task SetAsync(
        [Summary(description: "Date of your birthday [MM/DD]")] string date,
        [Summary(description: "Your time zone, eg Eastern Standard Time, Google 'What is my time zone' for help")] string timeZone)
    {
        // takes a date and timezone, responds with a button interaction and a timestamp, for the user to verify if it's the correct date/time

        if (!BirthdatePattern.Parse(date).TryGetValue(new AnnualDate(1, 1), out var birthDate))
        {
            await RespondBasicAsync("Couldn't parse birthdate", "Date should be in month/day format, for example `04/14`", Color.Red);
            return;
        }

        // Need to convert the parsedBirthDate + the parsed time-zone, to the EST equiv at 8am (configurable time), then take that and present the timestamp
        // we also need to present the timestamp for when their birthday actual is
        // so one timestamp for the day of their birthday
        // then the timestamp for when it'll be announced
        // this gets weird with the year

        var birthDateTimezone = FindDateTimeZone(timeZone);
        if (birthDateTimezone is null)
        {
            var stringBuilder = new StringBuilder()
                .Append("`" + birthDateTimezone + "` wasn't matched to a time-zone")
                .AppendLine()
                .AppendLine()
                .Append("Visit [this timezone tool](https://webbrowsertools.com/timezone) and try passing the 'Timezone' field into the command, or [try google](https://www.google.com/search?q=whats+is+my+timezone)");

            await RespondBasicAsync("Time-zone not found", stringBuilder.ToString(), Color.Red);
            return;
        }

        var result = await _userService.UpsertAsync(Context.User.Id, doc =>
        {
            doc.AnnualBirthdate = birthDate;
            doc.Timezone = birthDateTimezone.Id;
            doc.EnableBirthday = false;
        });

        if (result.IsFailed)
        {
            await RespondFailureAsync(result.Errors);
            return;
        }

        var components = new ComponentBuilder()
            .WithButton(new ButtonBuilder().WithLabel("Yes").WithCustomId("tz:y").WithStyle(ButtonStyle.Primary))
            .WithButton(new ButtonBuilder().WithLabel("No").WithCustomId("tz:n").WithStyle(ButtonStyle.Secondary))
            .Build();

        var currentDateInTimezone = _clock.InZone(birthDateTimezone).GetCurrentDate();
        var birthDateTime = new LocalDateTime(currentDateInTimezone.Year, birthDate.Month, birthDate.Day, 0, 0).InZoneLeniently(birthDateTimezone);
        var birthDateTimeTimeOffset = birthDateTime.ToDateTimeOffset();

        var description = new StringBuilder()
            .Append("You're birthday is/was ").Append(birthDateTimeTimeOffset.ToDiscordTimestamp(TimestampTagStyles.Relative))
            .Append(" on ").Append(birthDateTimeTimeOffset.ToDiscordTimestamp(TimestampTagStyles.ShortDate));

        var field = new EmbedFieldBuilder()
            .WithName("Timezone")
            .WithValue(birthDateTimezone.Id)
            .WithIsInline(false);

        var embed = EmbedHelper.Basic(
            description: description.ToString(),
            authorName: Context.User.Username,
            authorIcon: Context.User.GetAvatarUrl(),
            color: Color.Magenta,
            fields: new List<EmbedFieldBuilder> { field }).Build();

        await RespondAsync(embed: embed, components: components, ephemeral: true);
    }

    [SlashCommand("clear", "Clears your birthday")]
    public async Task ClearAsync()
    {
        var result = await _userService.UpsertAsync(Context.User.Id, doc =>
        {
            doc.Timezone = null;
            doc.AnnualBirthdate = null;
        });

        if (result.IsFailed)
        {
            await RespondFailureAsync(result.Errors);
            return;
        }

        await RespondSuccessAsync("Cleared your birthday & stored time-zone");
    }

    [ComponentInteraction("tz:*", true)]
    public async Task HandleTimeZoneAsync(string confirm)
    {
        await DeferAsync(ephemeral: true);

        var confirmed = confirm == "y";

        if (confirmed)
        {
            var result = await _userService.UpsertAsync(Context.User.Id, doc => doc.EnableBirthday = true);
            if (result.IsFailed)
            {
                await ModifyOriginalResponseToFailureAsync(result.Errors);
                return;
            }

            await ModifyOriginalResponseToSuccessAsync("Birthday set & enabled");
            return;
        }

        await Context.Interaction.DeleteOriginalResponseAsync();
    }

    private DateTimeZone? FindDateTimeZone(string timeZone)
    {
        // TitleCase the timezone, so we can parse it for either iana or windows
        timeZone = timeZone.ToTitleCase(Context.Interaction.UserLocale);

        // Try to parse windows time-zone
        if (TZConvert.TryWindowsToIana(timeZone, out var ianaTimeZoneId))
        {
            return DateTimeZoneProviders.Tzdb.GetZoneOrNull(ianaTimeZoneId);
        }

        // Try to parse iana time-zone
        var ianaTimeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(timeZone);
        ianaTimeZone ??= DateTimeZoneProviders.Tzdb.GetZoneOrNull(timeZone.ToUpperInvariant()); // Capitalize, maybe it's an abbreviation

        return ianaTimeZone;
    }
}
