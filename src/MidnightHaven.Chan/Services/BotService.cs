using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MidnightHaven.Chan.Consumers;
using SlimMessageBus;

namespace MidnightHaven.Chan.Services;

public partial class BotService : BackgroundService
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _interactionService;
    private readonly IMessageBus _bus;
    private readonly IServiceProvider _serviceProvider;
    private readonly DiscordOptions _discordOptions;
    private readonly ILogger<BotService> _logger;

    public BotService(
        DiscordSocketClient client,
        InteractionService interactionService,
        IMessageBus bus,
        IServiceProvider serviceProvider,
        IOptions<DiscordOptions> discordOptions,
        ILogger<BotService> logger)
    {
        _client = client;
        _interactionService = interactionService;
        _bus = bus;
        _serviceProvider = serviceProvider;
        _discordOptions = discordOptions.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (string.IsNullOrEmpty(_discordOptions.Token))
        {
            throw new ArgumentNullException(nameof(_discordOptions.Token), "Discord token cannot be empty or null");
        }

        _client.Log += LogAsync;
        _client.Ready += ReadyAsync;
        _interactionService.Log += LogAsync;

        // Register our command/interaction modules to our DI
        await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);

        await _client.LoginAsync(TokenType.Bot, _discordOptions.Token);
        await _client.StartAsync();

        // Publish events to our Slim Message Bus consumers
        _client.GuildScheduledEventCreated += @event => _bus.Publish(@event, Topics.GuildEvent.Created, cancellationToken: stoppingToken);
        _client.GuildScheduledEventStarted += @event => _bus.Publish(@event, Topics.GuildEvent.Started, cancellationToken: stoppingToken);
        _client.GuildScheduledEventCancelled += @event => _bus.Publish(@event, Topics.GuildEvent.Cancelled, cancellationToken: stoppingToken);

        _client.InteractionCreated += HandleInteractionAsync;

        await Task.Delay(-1, stoppingToken);
    }

    private async Task ReadyAsync()
    {
        // Register commands directly to a guild or globally
        // If we have a target guild, this will register our commands immediately, instead of a ~15 minute delay
        if (_discordOptions.Guild is not null)
        {
            await _interactionService.RegisterCommandsGloballyAsync(); // sync up commands anyway
            await _interactionService.RegisterCommandsToGuildAsync(_discordOptions.Guild.Value);
        }
        else
        {
            await _interactionService.RegisterCommandsGloballyAsync();
        }
    }

    private async Task HandleInteractionAsync(SocketInteraction interaction)
    {
        try
        {
            // Create an execution context that matches the generic type parameter of your InteractionModuleBase<T> modules.
            var context = new SocketInteractionContext(_client, interaction);

            // Execute the incoming command.
            var result = await _interactionService.ExecuteCommandAsync(context, _serviceProvider);

            if (!result.IsSuccess)
            {
                switch (result.Error)
                {
                    case InteractionCommandError.UnmetPrecondition:
                        // implement
                        break;
                    default:
                        break;
                }
            }
        }
        catch
        {
            // If Slash Command execution fails it is most likely that the original interaction acknowledgement will persist. It is a good idea to delete the original
            // response, or at least let the user know that something went wrong during the command execution.
            if (interaction.Type is InteractionType.ApplicationCommand)
            {
                await interaction.GetOriginalResponseAsync().ContinueWith(async msg => await msg.Result.DeleteAsync());
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _client.StopAsync();
    }

    // Birthday system
    // Fact of the day system
    // Announce when events are about to start - maybe loop through every 5 minutes? would be an easy low-maintenance background service, could use the user timezone thing

    private async Task LogAsync(LogMessage message)
    {
        switch (message.Severity)
        {
            case LogSeverity.Critical: LogCritical(message.Exception, message.Source, message.Message); break;
            case LogSeverity.Error: LogError(message.Exception, message.Source, message.Message); break;
            case LogSeverity.Warning: LogWarning(message.Exception, message.Source, message.Message); break;
            case LogSeverity.Info: LogInformation(message.Exception, message.Source, message.Message); break;
            case LogSeverity.Verbose: LogTrace(message.Exception, message.Source, message.Message); break;
            case LogSeverity.Debug: LogDebug(message.Exception, message.Source, message.Message); break;
            default: LogInformation(message.Exception, message.Source, message.Message); break;
        }
        await Task.CompletedTask;
    }

    [LoggerMessage(EventId = 0, Level = LogLevel.Critical, Message = "[{source}] {message}")]
    public partial void LogCritical(Exception ex, string source, string message);

    [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "[{source}] {message}")]
    public partial void LogError(Exception ex, string source, string message);

    [LoggerMessage(EventId = 2, Level = LogLevel.Warning, Message = "[{source}] {message}")]
    public partial void LogWarning(Exception ex, string source, string message);

    [LoggerMessage(EventId = 3, Level = LogLevel.Trace, Message = "[{source}] {message}")]
    public partial void LogTrace(Exception ex, string source, string message);

    [LoggerMessage(EventId = 4, Level = LogLevel.Debug, Message = "[{source}] {message}")]
    public partial void LogDebug(Exception ex, string source, string message);

    [LoggerMessage(EventId = 5, Level = LogLevel.Information, Message = "[{source}] {message}")]
    public partial void LogInformation(Exception ex, string source, string message);
}
