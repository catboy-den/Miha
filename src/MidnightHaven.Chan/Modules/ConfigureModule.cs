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
public class ConfigureModule : InteractionModuleBase<SocketInteractionContext>
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

    [EnabledInDm(true)]
    [DefaultMemberPermissions(GuildPermission.ManageChannels)]
    [SlashCommand("configure", "Configure various bot settings and options")]
    public async Task ConfigureAsync(
        [Summary("eventLoggingChannel", "The channel event Creation, Changes, or Cancellations will be logged to")] ITextChannel? logChannel = null,
        [Summary("eventAnnouncementChannel", "The channel event start and soon-to-start Announcements will be posted to")] ITextChannel? announcementChannel = null)
    {
        var options = new GuildOptions
        {
            GuildId = Context.Guild.Id,
            AnnouncementChannel = announcementChannel?.Id,
            LogChannel = logChannel?.Id
        };

        var result = await _guildOptionsService.UpsertAsync(options);

        if (result.IsSuccess)
        {
            var fields = new List<EmbedFieldBuilder>
            {
                EmbedFieldHelper.TextChannel("Announcement channel",
                        result.Value?.AnnouncementChannel != null ? (await _client.GetChannelAsync(result.Value.AnnouncementChannel.Value)).Name : null)
                    .WithIsInline(true),

                EmbedFieldHelper.TextChannel("Event logging channel",
                        result.Value?.LogChannel != null ? (await _client.GetChannelAsync(result.Value.LogChannel.Value)).Name : null)
                    .WithIsInline(true)
            };

            var embed = EmbedHelper.Success(
                    description: "Updated Guild Options",
                    authorName: Context.User.Username,
                    authorIcon: Context.User.GetAvatarUrl())
                .WithFields(fields)
                .Build();

            await RespondAsync(embed: embed, ephemeral: true);
        }
        else
        {
            var embed = EmbedHelper.Failure(
                    description: "Updating Guild Options",
                    authorName: Context.User.Username,
                    authorIcon: Context.User.GetAvatarUrl(),
                    errors: result.Errors)
                .Build();

            await RespondAsync(embed: embed, ephemeral: true);
        }
    }
}
