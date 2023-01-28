using Discord;
using Discord.Interactions;

namespace MidnightHaven.Chan.Modules;

public class ConfigureModule : InteractionModuleBase<SocketInteractionContext>
{
    public ConfigureModule()
    {

    }

    [EnabledInDm(false)]
    [DefaultMemberPermissions(GuildPermission.ManageChannels)]
    [SlashCommand("configure", "Configure various bot settings and options")]
    public async Task ConfigureAsync(
        [Summary("loggingChannel", "The channel the bot will log to when an event is Created, Cancelled, or Modified")] IChannel? logChannel = null,
        [Summary("announcementChannel", "The channel the bot will post event Start and Soon-To-Start Announcements to")] IChannel? announcementChannel = null)
    {

    }
}
