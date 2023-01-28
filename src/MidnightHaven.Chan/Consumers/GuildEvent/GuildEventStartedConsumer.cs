using Discord;
using SlimMessageBus;

namespace MidnightHaven.Chan.Consumers.GuildEvents;

public class GuildEventStartedConsumer : IConsumer<IGuildScheduledEvent>
{
    public async Task OnHandle(IGuildScheduledEvent message)
    {
        throw new NotImplementedException();
    }
}
