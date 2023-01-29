using Discord;
using SlimMessageBus;

namespace MidnightHaven.Chan.Consumers.GuildEvent;

public class GuildEventUpdatedConsumer : IConsumer<IGuildScheduledEvent>
{
    public async Task OnHandle(IGuildScheduledEvent message)
    {
        throw new NotImplementedException();
    }
}
