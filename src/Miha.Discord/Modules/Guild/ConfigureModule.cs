using Discord;
using Discord.Interactions;
using Miha.Logic.Services.Interfaces;
using Miha.Redis.Documents;

namespace Miha.Discord.Modules.Guild;

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

    [SlashCommand("logging", "Sets or updates the event logging channel, where any event modifications are logged")]
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
            [Summary(description: "The channel where announcements will be posted")]
            [ChannelTypes(ChannelType.News, ChannelType.Text)] ITextChannel channel,
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

    [Group("birthdays", "Set or update birthday settings and options")]
    public class BirthdayModule : BaseInteractionModule
    {
        private readonly IGuildService _guildService;

        public BirthdayModule(IGuildService guildService)
        {
            _guildService = guildService;
        }

        [SlashCommand("channel", "Sets or updates the channel where birthday announcements will be posted")]
        public async Task ChannelAsync(
            [Summary(description: "The channel where today's birthday will be posted")]
            [ChannelTypes(ChannelType.News, ChannelType.Text)] IChannel channel,
            [Summary(description: "Setting this to true will disable announcements, even if you set a channel")] bool disable = false)
        {
            var textChannel = channel as ITextChannel;

            var result = await _guildService.UpsertAsync(textChannel.GuildId, options => options.BirthdayAnnouncementChannel = disable ? null : channel.Id);

            if (result.IsFailed)
            {
                await RespondErrorAsync(result.Errors);
                return;
            }

            var fields = new EmbedFieldBuilder()
                .WithName("Birthday announcement channel")
                .WithValue(disable ? "Disabled" : textChannel.Mention);

            await RespondSuccessAsync("Updated Birthday announcement Channel", fields);
        }

        [SlashCommand("roles", "Update the list of roles the birthday announcer is allowed to announce birthdays for")]
        public async Task RolesAsync(
            [Summary(description: "Role to add to the list, if a user has any roles in the list, their birthday can be announced")] IRole? add = null,
            [Summary(description: "Role to remove from the list, if the list is empty any user can have their birthday announced")] IRole? remove = null)
        {
            var guildDocResult = await _guildService.GetAsync(Context.Guild.Id);

            if (guildDocResult.IsFailed)
            {
                await RespondErrorAsync(guildDocResult.Errors);
                return;
            }

            var guildDoc = guildDocResult.Value ?? new GuildDocument { Id = Context.Guild.Id };

            if (add is null && remove is null)
            {
                // respond with just the list
                return;
            }

            guildDoc.BirthdayAnnouncementRoles ??= new List<ulong>();

            if (add is not null && !guildDoc.BirthdayAnnouncementRoles.Contains(add.Id))
            {
                guildDoc.BirthdayAnnouncementRoles.Add(add.Id);
            }

            if (remove is not null)
            {
                guildDoc.BirthdayAnnouncementRoles.RemoveAll(roleId => roleId == remove.Id);
            }

            var result = await _guildService.UpsertAsync(guildDoc);

            if (result.IsFailed)
            {
                await RespondErrorAsync(guildDocResult.Errors);
                return;
            }

            // respond with success + the list
        }
    }
}
