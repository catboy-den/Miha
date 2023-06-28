using Discord;
using Discord.Addons.Hosting;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Miha;
using Miha.Discord;
using Serilog;
using Serilog.Exceptions;
using Serilog.Formatting.Compact;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithExceptionDetails()
    .WriteTo.Console(new CompactJsonFormatter())
    .CreateLogger();

try
{
    Log.Information("Starting host");

    var host = Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration(builder => builder.AddEnvironmentVariables())
        .UseSerilog()
        .ConfigureDiscordHost((context, config) =>
        {
            var discordOptions = context.Configuration.GetSection(DiscordOptions.Section).Get<DiscordOptions>();

            if (string.IsNullOrEmpty(discordOptions?.Token))
            {
                throw new ArgumentNullException(nameof(discordOptions.Token), "Discord token cannot be empty or null");
            }

            if (discordOptions.Guild is null)
            {
                throw new ArgumentNullException(nameof(discordOptions.Guild), "We need a target guild id, we don't quite support multi-guilds yet");
            }

            config.SocketConfig = new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info,
                AlwaysDownloadUsers = true,
                GatewayIntents = GatewayIntents.GuildScheduledEvents
                                 | GatewayIntents.DirectMessageTyping
                                 | GatewayIntents.DirectMessageReactions
                                 | GatewayIntents.DirectMessages
                                 | GatewayIntents.GuildMessageTyping
                                 | GatewayIntents.GuildMessageReactions
                                 | GatewayIntents.GuildMessages
                                 | GatewayIntents.GuildVoiceStates
                                 | GatewayIntents.Guilds
                                 | GatewayIntents.GuildMembers
            };

            config.Token = discordOptions.Token;
        })
        .UseInteractionService((_, config) =>
        {
            config.LogLevel = LogSeverity.Info;
            config.UseCompiledLambda = true;
        })
        .ConfigureServices(Startup.ConfigureServices).Build();

    await host.RunAsync();
    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
    return 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}
