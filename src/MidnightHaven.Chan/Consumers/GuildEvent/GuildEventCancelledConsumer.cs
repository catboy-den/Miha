using Discord;
using Discord.WebSocket;
using SlimMessageBus;

namespace MidnightHaven.Chan.Consumers.GuildEvents;

public class GuildEventCancelledConsumer : IConsumer<IGuildScheduledEvent>
{
    private readonly DiscordSocketClient _client;

    public GuildEventCancelledConsumer(DiscordSocketClient client)
    {
        _client = client;
    }

    public async Task OnHandle(IGuildScheduledEvent message)
    {
        var channel = await _client.GetChannelAsync(1068697078152314913);

        await (channel as IMessageChannel)!.SendMessageAsync(message.Name + " cancelled");
    }
}
