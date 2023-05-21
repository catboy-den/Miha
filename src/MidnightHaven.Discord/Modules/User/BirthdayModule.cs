using System.Text;
using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Logging;
using MidnightHaven.Discord.Extensions;
using MidnightHaven.Logic.Services.Interfaces;
using NodaTime;
using NodaTime.Extensions;
using NodaTime.Text;
using TimeZoneConverter;

namespace MidnightHaven.Discord.Modules.User;

/// <summary>
/// Birthday related interactions
/// </summary>
[Group("birthday", "Birthday related commands")]
public class BirthdayModule : BaseInteractionModule
{
    private static readonly AnnualDatePattern BirthdatePattern = AnnualDatePattern.CreateWithInvariantCulture("MM/dd");

    private readonly IClock _clock;
    private readonly IUserService _userService;
    private readonly IBirthdayJobService _birthdayJobService;
    private readonly ILogger<BirthdayModule> _logger;

    public BirthdayModule(
        IClock clock,
        IUserService userService,
        IBirthdayJobService birthdayJobService,
        ILogger<BirthdayModule> logger)
    {
        _clock = clock;
        _userService = userService;
        _birthdayJobService = birthdayJobService;
        _logger = logger;
    }

    [SlashCommand("get", "Gets your, or another users, birthday")]
    public async Task GetAsync(IUser? user = null)
    {
        var targetUser = user ?? Context.User;
        var result = await _userService.GetAsync(targetUser.Id);
        var userDoc = result.Value;
        var userTimezone = userDoc?.Timezone;

        if (result.IsFailed)
        {
            await RespondErrorAsync(result.Errors);
            return;
        }

        if (userDoc is null
            || userDoc.EnableBirthday is false
            || userDoc.AnnualBirthdate is null
            || userTimezone is null)
        {
            var noBirthdayEmbed = new EmbedBuilder().AsMinimal(
                    targetUser.Username,
                    targetUser.GetAvatarUrl(),
                    targetUser.Mention + " doesn't have a birthday set");

            await RespondAsync(embed: noBirthdayEmbed.Build(), ephemeral: true);
            return;
        }

        var userBirthdate = userDoc.AnnualBirthdate.Value;

        var currentDateInTimezone = _clock.InZone(userTimezone).GetCurrentDate();
        var birthDateTime = new LocalDateTime(currentDateInTimezone.Year, userBirthdate.Month, userBirthdate.Day, 0, 0).InZoneLeniently(userTimezone);
        var birthDateTimeTimeOffset = birthDateTime.ToDateTimeOffset();

        var birthdayAlreadyHappened = _clock.GetCurrentInstant().ToUnixTimeMilliseconds() > birthDateTimeTimeOffset.ToUnixTimeMilliseconds();

        var description = new StringBuilder()
            .Append(targetUser.Mention)
            .Append("'s birthday").Append(birthdayAlreadyHappened ? " was " : " is ").Append(birthDateTimeTimeOffset.ToDiscordTimestamp(TimestampTagStyles.Relative))
            .Append(" on ").Append(birthDateTimeTimeOffset.ToDiscordTimestamp(TimestampTagStyles.ShortDate));

        var field = new EmbedFieldBuilder()
            .WithName("Timezone")
            .WithValue(userTimezone.Id)
            .WithIsInline(false);

        var embed = new EmbedBuilder().AsMinimal(
            Context.User.Username,
            Context.User.GetAvatarUrl(),
            description.ToString(),
            field);

        await RespondAsync(embed: embed.Build(), ephemeral: true);
    }

    [SlashCommand("set", "Sets or updates your birthday")]
    public async Task SetAsync(
        [Summary(description: "Date of your birthday [MM/DD]")] string date,
        [Summary(description: "Your time zone, eg Eastern Standard Time, Google 'What is my time zone' for help")] string timeZone)
    {
        if (!BirthdatePattern.Parse(date).TryGetValue(new AnnualDate(1, 1), out var birthDate))
        {
            await RespondMinimalAsync("Couldn't parse birthdate, date should be in month/date format, for example `04/14`");
            return;
        }

        var birthDateTimezone = FindDateTimeZone(timeZone);
        if (birthDateTimezone is null)
        {
            var stringBuilder = new StringBuilder()
                .Append("`" + timeZone + "` wasn't matched to a time-zone")
                .AppendLine()
                .AppendLine()
                .Append("Visit [this timezone tool](https://webbrowsertools.com/timezone) and try passing the `Timezone` field into the command, or [try google](https://www.google.com/search?q=whats+is+my+timezone)");

            await RespondFailureAsync(stringBuilder.ToString());
            return;
        }

        var result = await _userService.UpsertAsync(Context.User.Id, doc =>
        {
            doc.AnnualBirthdate = birthDate;
            doc.Timezone = birthDateTimezone;
            doc.EnableBirthday = false;
            doc.LastBirthdateAnnouncement = null;
        });

        if (result.IsFailed)
        {
            await RespondErrorAsync(result.Errors);
            return;
        }

        var components = new ComponentBuilder()
            .WithButton(new ButtonBuilder().WithLabel("Yes").WithCustomId("tz:y").WithStyle(ButtonStyle.Primary))
            .WithButton(new ButtonBuilder().WithLabel("No").WithCustomId("tz:n").WithStyle(ButtonStyle.Secondary))
            .Build();

        var currentDateInTimezone = _clock.InZone(birthDateTimezone).GetCurrentDate();
        var birthDateTime = new LocalDateTime(currentDateInTimezone.Year, birthDate.Month, birthDate.Day, 0, 0).InZoneLeniently(birthDateTimezone);
        var birthDateTimeTimeOffset = birthDateTime.ToDateTimeOffset();

        var birthdayAlreadyHappened = _clock.GetCurrentInstant().ToUnixTimeMilliseconds() > birthDateTimeTimeOffset.ToUnixTimeMilliseconds();

        var description = new StringBuilder()
            .Append("You're birthday ").Append(birthdayAlreadyHappened ? " was " : " is ").Append(birthDateTimeTimeOffset.ToDiscordTimestamp(TimestampTagStyles.Relative))
            .Append(" on ").Append(birthDateTimeTimeOffset.ToDiscordTimestamp(TimestampTagStyles.ShortDate));

        var field = new EmbedFieldBuilder()
            .WithName("Timezone")
            .WithValue(birthDateTimezone.Id)
            .WithIsInline(false);

        var embed = new EmbedBuilder().AsMinimal(
            Context.User.Username,
            Context.User.GetAvatarUrl(),
            description.ToString(),
            field);

        await RespondAsync(embed: embed.Build(), components: components, ephemeral: true);
    }

    [SlashCommand("clear", "Clears your birthday")]
    public async Task ClearAsync()
    {
        var deleteJobResult = await _birthdayJobService.DeleteAsync(Context.User.Id, true);

        if (deleteJobResult.IsFailed)
        {
            await RespondErrorAsync(deleteJobResult.Errors);
            return;
        }

        var result = await _userService.UpsertAsync(Context.User.Id, doc =>
        {
            doc.EnableBirthday = false;
            doc.Timezone = null;
            doc.AnnualBirthdate = null;
            doc.LastBirthdateAnnouncement = null;
        });

        if (result.IsFailed)
        {
            await RespondErrorAsync(result.Errors);
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
            var deleteJobResult = await _birthdayJobService.DeleteAsync(Context.User.Id, true);

            if (deleteJobResult.IsFailed)
            {
                await ModifyOriginalResponseToErrorAsync(deleteJobResult.Errors);
                return;
            }

            var result = await _userService.UpsertAsync(Context.User.Id, doc => doc.EnableBirthday = true);
            if (result.IsFailed)
            {
                await ModifyOriginalResponseToErrorAsync(result.Errors);
                return;
            }

            await ModifyOriginalResponseToSuccessAsync("Birthday set & enabled");
            return;
        }

        await Context.Interaction.DeleteOriginalResponseAsync();
    }

    private DateTimeZone? FindDateTimeZone(string? timeZone)
    {
        if (string.IsNullOrEmpty(timeZone))
        {
            return null;
        }

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
