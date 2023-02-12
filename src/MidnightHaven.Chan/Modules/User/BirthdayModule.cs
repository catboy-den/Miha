using Discord;
using Discord.Interactions;
using MidnightHaven.Chan.Services.Logic.Interfaces;

namespace MidnightHaven.Chan.Modules.User;

/// <summary>
/// Birthday related interactions
/// </summary>
[Group("birthday", "Birthday related commands")]
public class BirthdayModule : BaseInteractionModule
{
    private readonly IUserService _userService;

    public BirthdayModule(IUserService userService)
    {
        _userService = userService;
    }

    [SlashCommand("get", "Gets your set birthday information")]
    public async Task GetAsync()
    {

    }

    [SlashCommand("set", "Sets or updates your birthday")]
    public async Task SetAsync(string timeZone)
    {
        try
        {
            //var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            var components = new ComponentBuilder()
                .WithButton(new ButtonBuilder()
                    .WithLabel("Yes")
                    .WithCustomId("tz")
                    .WithStyle(ButtonStyle.Primary));

            await RespondAsync("Press a button", components: components.Build());
        }
        catch (TimeZoneNotFoundException e)
        {
            return;
        }
    }

    [SlashCommand("clear", "Clears your birthday information")]
    public async Task ClearAsync()
    {

    }

    [SlashCommand("timezones", "Sets or updates your birthday")]
    public async Task TimeZonesAsync(string timeZone)
    {

    }

    [ComponentInteraction("tz")]
    public async Task HandleTimeZoneAsync()
    {
        await RespondAsync("recieved");
    }
}
