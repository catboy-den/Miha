using System.Text;
using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Logging;
using MidnightHaven.Chan.Extensions;
using MidnightHaven.Chan.Services.Logic.Interfaces;
using NodaTime;
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

    private readonly IUserService _userService;
    private readonly ILogger<BirthdayModule> _logger;

    public BirthdayModule(
        IUserService userService,
        ILogger<BirthdayModule> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [SlashCommand("get", "Gets your, or another users, birthday")]
    public async Task GetAsync(IUser? user = null)
    {
        // returns a timestamp to the users birthday (of this year)
    }

    [SlashCommand("set", "Sets or updates your birthday")]
    public async Task SetAsync(
        [Summary(description: "Date of your birthday [MM/DD]")] string date,
        [Summary(description: "Your time zone, eg Eastern Standard Time, Google 'What is my time zone' for help")] string timeZone)
    {
        // takes a date and timezone, responds with a button interaction and a timestamp, for the user to verify if it's the correct date/time

        if (!BirthdatePattern.Parse(date).TryGetValue(new AnnualDate(1, 1), out var parsedBirthDate))
        {
            await RespondBasicAsync("Couldn't parse birthdate", "Date should be in month/day format, for example `04/14`", Color.Red);
            return;
        }

        // Need to convert the parsedBirthDate + the parsed time-zone, to the EST equiv at 8am (configurable time), then take that and present the timestamp
        // we also need to present the timestamp for when their birthday actual is
        // so one timestamp for the day of their birthday
        // then the timestamp for when it'll be announced
        // this gets weird with the year

        var resolvedTimeZone = FindDateTimeZone(timeZone);
        if (resolvedTimeZone is null)
        {
            var stringBuilder = new StringBuilder()
                .Append("`" + timeZone + "` wasn't matched to a time-zone")
                .AppendLine()
                .AppendLine()
                .Append("Visit [this timezone tool](https://webbrowsertools.com/timezone) and try passing the 'Timezone' field into the command, or [try google](https://www.google.com/search?q=whats+is+my+timezone)");

            await RespondBasicAsync("Time-zone not found", stringBuilder.ToString(), Color.Red);
            return;
        }

        var result = await _userService.UpsertAsync(Context.User.Id, doc =>
        {
            doc.AnnualBirthdate = parsedBirthDate;
            doc.Timezone = resolvedTimeZone.Id;
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

        await RespondAsync("", components: components, ephemeral: true);
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

            await ModifyOriginalResponseAsync(properties => properties.Content = "Woop");
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
