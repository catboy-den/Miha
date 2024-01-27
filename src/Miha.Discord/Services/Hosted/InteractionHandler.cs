using System.Reflection;
using Discord;
using Discord.Addons.Hosting;
using Discord.Addons.Hosting.Util;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Miha.Discord.Services.Hosted;

public class InteractionHandler(
    DiscordSocketClient client,
    IServiceProvider provider,
    InteractionService interactionService,
    IOptions<DiscordOptions> discordOptions,
    ILogger<InteractionHandler> logger) : DiscordClientService(client, logger)
{
    private readonly ILogger<DiscordClientService> _logger = logger;
    private readonly DiscordOptions _discordOptions = discordOptions.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_discordOptions.Guild is null)
        {
            throw new ArgumentNullException(nameof(_discordOptions.Guild), "We need a target guild id, multi-guilds aren't quite supported yet");
        }

        Client.InteractionCreated += HandleInteraction;

        await interactionService.AddModulesAsync(Assembly.GetExecutingAssembly(), provider);

        await Client.WaitForReadyAsync(stoppingToken);

        await interactionService.RegisterCommandsToGuildAsync(_discordOptions.Guild.Value);
    }

    private async Task HandleInteraction(SocketInteraction arg)
    {
        try
        {
            // Create an execution context that matches the generic type parameter of your InteractionModuleBase<T> modules
            var ctx = new SocketInteractionContext(Client, arg);
            await interactionService.ExecuteCommandAsync(ctx, provider);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred whilst attempting to handle interaction");

            if (arg.Type == InteractionType.ApplicationCommand)
            {
                var msg = await arg.GetOriginalResponseAsync();
                await msg.DeleteAsync();
            }

        }
    }
}
