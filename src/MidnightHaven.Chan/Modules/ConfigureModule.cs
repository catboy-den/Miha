using System.Text.Json;
using Discord;
using Discord.Interactions;
using MidnightHaven.Redis.Models;
using MidnightHaven.Redis.Services.Interfaces;

namespace MidnightHaven.Chan.Modules;

/// <summary>
/// Command module for configuring a guilds <see cref="GuildOptions"/>
/// </summary>
public class ConfigureModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IGuildOptionsService _guildOptionsService;

    public ConfigureModule(IGuildOptionsService guildOptionsService)
    {
        _guildOptionsService = guildOptionsService;
    }

    [EnabledInDm(false)]
    [DefaultMemberPermissions(GuildPermission.ManageChannels)]
    [SlashCommand("configure", "Configure various bot settings and options")]
    public async Task ConfigureAsync(
        [Summary("eventLoggingChannel", "The channel event Creation, Changes, or Cancellations will be logged to")] ITextChannel? logChannel = null,
        [Summary("eventAnnouncementChannel", "The channel event start and soon-to-start Announcements will be posted to")] ITextChannel? announcementChannel = null)
    {
        var options = new GuildOptions
        {
            GuildId = Context.Guild.Id,
            LogChannel = logChannel?.Id,
            AnnouncementChannel = announcementChannel?.Id
        };

        var result = await _guildOptionsService.UpsertAsync(options);

        if (result.IsSuccess)
        {
            var jsonText = JsonSerializer.Serialize(result.Value);
            await RespondAsync(jsonText);
        }
        else
        {
            await RespondAsync(result.Errors.First().Message);
        }
    }
}
