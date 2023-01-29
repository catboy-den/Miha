using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using MidnightHaven.Chan.Helpers;
using MidnightHaven.Redis.Models;
using MidnightHaven.Redis.Services.Interfaces;

namespace MidnightHaven.Chan.Modules;

/// <summary>
/// Command module for configuring a guilds <see cref="GuildOptions"/>
/// </summary>
[Group("configure", "Set or update various bot settings and options")]
public class ConfigureModule : HavenModule
{
    private readonly DiscordSocketClient _client;
    private readonly IGuildOptionsService _guildOptionsService;

    public ConfigureModule(
        DiscordSocketClient client,
        IGuildOptionsService guildOptionsService)
    {
        _client = client;
        _guildOptionsService = guildOptionsService;
    }

    [SlashCommand("logging", "Sets or updates the event logging channel")]
    public async Task LoggingAsync(
        [Summary(description: "The channel any newly Created, Modified, or Cancelled events will be posted")] ITextChannel channel,
        [Summary(description: "Setting this to true will disable event logging, even if you set a channel")] bool disable = false)
    {
        var optionsResult = await _guildOptionsService.GetAsync(channel.GuildId);
        if (optionsResult.IsFailed)
        {
            await RespondFailureAsync(optionsResult.Errors);
            return;
        }

        var options = optionsResult.Value ?? new GuildOptions { GuildId = channel.GuildId };
        options.LogChannel = disable ? null : channel.Id;

        var upsertResult = await _guildOptionsService.UpsertAsync(options);
        if (upsertResult.IsFailed)
        {
            await RespondFailureAsync(upsertResult.Errors);
            return;
        }

        await RespondAsync(embed: EmbedHelper.Success(
                description: "Updated Guild Options",
                authorName: Context.User.Username,
                authorIcon: Context.User.GetAvatarUrl())
            .WithFields(EmbedFieldHelper.TextChannel("Event logging channel", disable ? null : channel.Name))
            .Build(), ephemeral: true);
    }

    [Group("announcements", "Set or update announcement settings and options")]
    public class AnnouncementModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly DiscordSocketClient _client;
        private readonly IGuildOptionsService _guildOptionsService;

        public AnnouncementModule(
            DiscordSocketClient client,
            IGuildOptionsService guildOptionsService)
        {
            _client = client;
            _guildOptionsService = guildOptionsService;
        }

        [SlashCommand("channel", "Sets or updates the channel where event announcements will be posted")]
        public async Task ChannelAsync(
            [Summary(description: "The channel where announcements will be posted")] ITextChannel channel,
            [Summary(description: "Setting this to true will disable announcements, even if you set a channel")] bool disable = false)
        {
            await RespondAsync(channel.Name);
        }

        [SlashCommand("role", "Sets or updates the role that will be pinged when a event has started")]
        public async Task NotifyRoleAsync(
            [Summary(description: "The role that will be pinged when an event is announced as starting")] IRole notifyRole,
            [Summary(description: "Setting this to true will disable role-pings, even if you set a role")] bool disable = false)
        {
            await RespondAsync(notifyRole.Name);
        }
    }
}
