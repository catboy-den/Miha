﻿using Discord;
using Discord.Addons.Hosting;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Miha;
using Miha.Discord;
using Miha.Redis.Services;
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
        .ConfigureAppConfiguration((context, builder) =>
        {
            if (context.HostingEnvironment.IsDevelopment())
            {
                builder.AddJsonFile("appsettings.edge.json", optional: true, reloadOnChange: false);
            }

            // Disable ReloadOnChange for all sources, we don't intend to support this
            // and it creates a lot of inotify issues on docker hosts running on linux
            foreach (var s in builder.Sources)
            {
                if (s is FileConfigurationSource source)
                    source.ReloadOnChange = false;
            }

            builder.AddEnvironmentVariables();
        })
        .UseSerilog((context, logger) => logger
            .ReadFrom.Configuration(context.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .WriteTo.Console(new CompactJsonFormatter()))
        .ConfigureDiscordHost((context, config) =>
        {
            var discordOptions = context.Configuration.GetSection(DiscordOptions.Section).Get<DiscordOptions>();

            if (string.IsNullOrEmpty(discordOptions?.Token))
            {
                throw new ArgumentNullException(nameof(discordOptions.Token), "Discord token cannot be empty or null");
            }

            if (discordOptions.Guild is null)
            {
                throw new ArgumentNullException(nameof(discordOptions.Guild), "Need a target guild id, Miha does not support multi-guilds yet");
            }

            config.SocketConfig = new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose,
                LogGatewayIntentWarnings = true,
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
            config.LogLevel = LogSeverity.Verbose;
            config.UseCompiledLambda = true;
        })
        .ConfigureServices(Startup.ConfigureServices)
        .Build();

    var discordOptions = host.Services.GetRequiredService<IOptions<DiscordOptions>>();
    var seedService = host.Services.GetRequiredService<RedisSeedService>();
    
    await seedService.SeedGuildAsync(discordOptions.Value.Guild);
    
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
