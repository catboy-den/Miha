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
    public async Task SetAsync()
    {

    }

    [SlashCommand("clear", "Clears your birthday information")]
    public async Task ClearAsync()
    {

    }
}
