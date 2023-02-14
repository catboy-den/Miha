using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Logging;
using MidnightHaven.Chan.Extensions;
using MidnightHaven.Chan.Services.Logic.Interfaces;
using NodaTime;
using TimeZoneConverter;

namespace MidnightHaven.Chan.Modules.User;

/// <summary>
/// Birthday related interactions
/// </summary>
[Group("birthday", "Birthday related commands")]
public class BirthdayModule : BaseInteractionModule
{
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

        var resolvedTimeZone = FindDateTimeZone(timeZone);
        if (resolvedTimeZone is null)
        {
            await RespondAsync("no", ephemeral: false);
            return;
        }

        await RespondAsync("timezone: " + resolvedTimeZone, ephemeral: false);
    }

    [SlashCommand("clear", "Clears your birthday")]
    public async Task ClearAsync()
    {
        // set timezone in user doc to null
    }

    [ComponentInteraction("tz:*:*", true)]
    public async Task HandleTimeZoneAsync(string confirm, string timeZoneId)
    {
        // handles yes/no button interaction, sets the user doc timezone

        await RespondAsync(confirm + ", " + timeZoneId);
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
