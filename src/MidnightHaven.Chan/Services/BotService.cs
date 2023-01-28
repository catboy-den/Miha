using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlimMessageBus;

namespace MidnightHaven.Chan.Services;

public partial class BotService : BackgroundService
{
    private readonly DiscordSocketClient _client;
    private readonly IMessageBus _bus;
    private readonly DiscordOptions _discordOptions;
    private readonly ILogger<BotService> _logger;

    public BotService(
        DiscordSocketClient client,
        IMessageBus bus,
        IOptions<DiscordOptions> discordOptions,
        ILogger<BotService> logger)
    {
        _client = client;
        _bus = bus;
        _discordOptions = discordOptions.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _client.Log += LogAsync;

        await _client.LoginAsync(TokenType.Bot, _discordOptions.Token);
        await _client.StartAsync();

        _client.GuildScheduledEventCreated += @event => _bus.Publish(@event, Topics.GuildEvent.Created, cancellationToken: stoppingToken);
        _client.GuildScheduledEventStarted += @event => _bus.Publish(@event, Topics.GuildEvent.Started, cancellationToken: stoppingToken);
        _client.GuildScheduledEventCancelled += @event => _bus.Publish(@event, Topics.GuildEvent.Cancelled, cancellationToken: stoppingToken);

        await Task.Delay(-1, stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _client.StopAsync();
    }

    // Announce when events are about to start - maybe loop through every 5 minutes? would be an easy low-maintenance background service, could use the user timezone thing
    // Subscribe to events, the bot will DM new events, have the option to only show by day or type of event, like hosted in either Discord or VRChat

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
