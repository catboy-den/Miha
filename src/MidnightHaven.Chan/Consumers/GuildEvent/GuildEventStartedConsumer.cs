using Discord;
using SlimMessageBus;

namespace MidnightHaven.Chan.Consumers.GuildEvent;

public class GuildEventStartedConsumer : IConsumer<IGuildScheduledEvent>
{
    public async Task OnHandle(IGuildScheduledEvent message)
    {

    }
}
