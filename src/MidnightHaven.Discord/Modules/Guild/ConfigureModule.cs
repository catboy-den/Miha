using Discord;
using Discord.Interactions;
using MidnightHaven.Logic.Services.Interfaces;
using MidnightHaven.Redis.Documents;

namespace MidnightHaven.Discord.Modules.Guild;

/// <summary>
/// Command module for configuring a guilds <see cref="GuildDocument"/>
/// </summary>
[Group("configure", "Set or update various bot settings and options")]
[DefaultMemberPermissions(GuildPermission.Administrator)]
public class ConfigureModule : BaseInteractionModule
{
    private readonly IGuildService _guildService;

    public ConfigureModule(IGuildService guildService)
    {
        _guildService = guildService;
    }

    [SlashCommand("logging", "Sets or updates the event logging channel")]
    public async Task LoggingAsync(
        [Summary(description: "The channel newly Created, Modified, or Cancelled events will be posted")] ITextChannel channel,
        [Summary(description: "Setting this to true will disable event logging")] bool disable = false)
    {
        var result = await _guildService.UpsertAsync(channel.GuildId, options => options.LogChannel = disable ? null : channel.Id);

        if (result.IsFailed)
        {
            await RespondErrorAsync(result.Errors);
            return;
        }

        var fields = new EmbedFieldBuilder()
            .WithName("Event logging channel")
            .WithValue(disable ? "Disabled" : channel.Mention);

        await RespondSuccessAsync("Updated guild options", fields);
    }

    [Group("announcements", "Set or update announcement settings and options")]
    public class AnnouncementModule : BaseInteractionModule
    {
        private readonly IGuildService _guildService;

        public AnnouncementModule(IGuildService guildService)
        {
            _guildService = guildService;
        }

        [SlashCommand("channel", "Sets or updates the channel where event announcements will be posted")]
        public async Task ChannelAsync(
            [Summary(description: "The channel where announcements will be posted")] ITextChannel channel,
            [Summary(description: "Setting this to true will disable announcements, even if you set a channel")] bool disable = false)
        {
            var result = await _guildService.UpsertAsync(channel.GuildId, options => options.AnnouncementChannel = disable ? null : channel.Id);

            if (result.IsFailed)
            {
                await RespondErrorAsync(result.Errors);
                return;
            }

            var fields = new EmbedFieldBuilder()
                .WithName("Announcement channel")
                .WithValue(disable ? "Disabled" : channel.Mention);

            await RespondSuccessAsync("Updated Announcement Channel", fields);
        }

        [SlashCommand("role", "Sets or updates the role that will be pinged when a event has started")]
        public async Task NotifyRoleAsync(
            [Summary(description: "The role that will be pinged when an event is announced as starting")] IRole notifyRole,
            [Summary(description: "Setting this to true will disable role-pings, even if you set a role")] bool disable = false)
        {
            var result = await _guildService.UpsertAsync(notifyRole.Guild.Id, options => options.AnnouncementRoleId = disable ? null : notifyRole.Id);

            if (result.IsFailed)
            {
                await RespondErrorAsync(result.Errors);
                return;
            }

            var fields = new EmbedFieldBuilder()
                .WithName("Notify role")
                .WithValue(disable ? "Disabled" : notifyRole.Mention);

            await RespondSuccessAsync("Updated Announcement Role", fields);
        }
    }
}
