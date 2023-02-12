using System.Reflection;
using Discord;
using Discord.Addons.Hosting;
using Discord.Addons.Hosting.Util;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MidnightHaven.Chan.Models.Options;

namespace MidnightHaven.Chan.Services.Handlers;

public class InteractionHandler : DiscordClientService
{
    private readonly IServiceProvider _provider;
    private readonly InteractionService _interactionService;
    private readonly ILogger<DiscordClientService> _logger;
    private readonly DiscordOptions _discordOptions;

    public InteractionHandler(
        DiscordSocketClient client,
        IServiceProvider provider,
        InteractionService interactionService,
        IOptions<DiscordOptions> discordOptions,
        ILogger<InteractionHandler> logger) : base(client, logger)
    {
        _provider = provider;
        _interactionService = interactionService;
        _logger = logger;
        _discordOptions = discordOptions.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_discordOptions.Guild is null)
        {
            throw new ArgumentNullException(nameof(_discordOptions.Guild), "We need a target guild id, multi-guilds aren't quite supported yet");
        }

        Client.InteractionCreated += HandleInteraction;

        await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);

        await Client.WaitForReadyAsync(stoppingToken);

        await _interactionService.RegisterCommandsToGuildAsync(_discordOptions.Guild.Value);
    }

    private async Task HandleInteraction(SocketInteraction arg)
    {
        try
        {
            // Create an execution context that matches the generic type parameter of your InteractionModuleBase<T> modules
            var ctx = new SocketInteractionContext(Client, arg);
            await _interactionService.ExecuteCommandAsync(ctx, _provider);
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
