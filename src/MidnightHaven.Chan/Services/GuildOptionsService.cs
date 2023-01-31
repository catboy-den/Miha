using Discord;
using Discord.WebSocket;
using FluentResults;
using Microsoft.Extensions.Logging;
using MidnightHaven.Chan.Services.Interfaces;
using MidnightHaven.Redis.Models;
using MidnightHaven.Redis.Repositories.Interfaces;

namespace MidnightHaven.Chan.Services;

public partial class GuildOptionsService : IGuildOptionsService
{
    private readonly DiscordSocketClient _client;
    private readonly IGuildOptionsRepository _repository;
    private readonly ILogger<GuildOptionsService> _logger;

    public GuildOptionsService(
        DiscordSocketClient client,
        IGuildOptionsRepository repository,
        ILogger<GuildOptionsService> logger)
    {
        _client = client;
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<GuildOptions?>> GetAsync(ulong? guildId)
    {
        try
        {
            if (guildId is null)
            {
                throw new ArgumentNullException(nameof(guildId));
            }

            return Result.Ok(await _repository.GetAsync(guildId));
        }
        catch (Exception e)
        {
            LogErrorException(e);
            return Result.Fail(e.Message);
        }
    }

    public async Task<Result<GuildOptions?>> UpsertAsync(GuildOptions options)
    {
        try
        {
            return Result.Ok(await _repository.UpsertAsync(options));
        }
        catch (Exception e)
        {
            LogErrorException(e);
            return Result.Fail(e.Message);
        }
    }

    public async Task<Result<GuildOptions?>> UpsertAsync(ulong? guildId, Action<GuildOptions> optionsFunc)
    {
        try
        {
            if (guildId is null)
            {
                throw new ArgumentNullException(nameof(guildId));
            }

            return Result.Ok(await _repository.UpsertAsync(guildId, optionsFunc));
        }
        catch (Exception e)
        {
            LogErrorException(e);
            return Result.Fail(e.Message);
        }
    }

    public async Task<Result> DeleteAsync(ulong? guildId)
    {
        try
        {
            if (guildId is null)
            {
                throw new ArgumentNullException(nameof(guildId));
            }

            await _repository.DeleteAsync(guildId);
            return Result.Ok();
        }
        catch (Exception e)
        {
            LogErrorException(e);
            return Result.Fail(e.Message);
        }
    }

    public async Task<Result<ITextChannel>> GetLoggingChannelAsync(ulong? guildId)
    {
        try
        {
            if (guildId is null)
            {
                throw new ArgumentNullException(nameof(guildId));
            }

            var optionsResult = await GetAsync(guildId);
            if (optionsResult.IsFailed)
            {
                _logger.LogWarning("Guild doesn't have any options when trying to get logging channel {GuildId}", guildId);
                return optionsResult.ToResult<ITextChannel>();
            }

            var logChannel = optionsResult.Value?.LogChannel;
            if (!logChannel.HasValue)
            {
                _logger.LogInformation("Guild doesn't have a logging channel set {GuildId}", guildId);
                return Result.Fail<ITextChannel>("Logging channel not set");
            }

            if (await _client.GetChannelAsync(logChannel.Value) is ITextChannel loggingChannel)
            {
                return Result.Ok(loggingChannel);
            }

            _logger.LogWarning("Guild's logging channel wasn't found, or might not be a Text Channel {GuildId} {LoggingChannelId}", guildId, logChannel.Value);
            return Result.Fail<ITextChannel>("Logging channel not found");
        }
        catch (Exception e)
        {
            LogErrorException(e);
            return Result.Fail<ITextChannel>(e.Message);
        }
    }

    public async Task<Result<ITextChannel>> GetAnnouncementChannelAsync(ulong? guildId)
    {
        try
        {
            if (guildId is null)
            {
                throw new ArgumentNullException(nameof(guildId));
            }

            var optionsResult = await GetAsync(guildId);
            if (optionsResult.IsFailed)
            {
                _logger.LogInformation("Guild doesn't have any options when trying to get announcement channel {GuildId}", guildId);
                return optionsResult.ToResult<ITextChannel>();
            }

            var announcementChannel = optionsResult.Value?.AnnouncementChannel;
            if (!announcementChannel.HasValue)
            {
                _logger.LogInformation("Guild doesn't have a announcement channel set {GuildId}", guildId);
                return Result.Fail<ITextChannel>("Announcement channel not set");
            }

            if (await _client.GetChannelAsync(announcementChannel.Value) is ITextChannel loggingChannel)
            {
                return Result.Ok(loggingChannel);
            }

            _logger.LogWarning("Guild's announcement channel wasn't found, or might not be a Text Channel {GuildId} {AnnouncementChannelId}", guildId, announcementChannel.Value);
            return Result.Fail<ITextChannel>("Announcement channel not found");
        }
        catch (Exception e)
        {
            LogErrorException(e);
            return Result.Fail<ITextChannel>(e.Message);
        }
    }

    public async Task<Result<IRole>> GetAnnouncementRoleAsync(ulong? guildId)
    {
        try
        {
            if (guildId is null)
            {
                throw new ArgumentNullException(nameof(guildId));
            }

            var optionsResult = await GetAsync(guildId);
            if (optionsResult.IsFailed)
            {
                _logger.LogInformation("Guild doesn't have any options when trying to get announcement role {GuildId}", guildId);
                return optionsResult.ToResult<IRole>();
            }

            var announcementRoleId = optionsResult.Value?.AnnouncementRoleId;
            if (!announcementRoleId.HasValue)
            {
                _logger.LogInformation("Guild doesn't have a announcement role set {GuildId}", guildId);
                return Result.Fail<IRole>("Announcement role not set");
            }

            if (_client.GetGuild(guildId.Value).GetRole(announcementRoleId.Value) is IRole announcementRole)
            {
                return Result.Ok(announcementRole);
            }

            _logger.LogWarning("Guild's announcement role wasn't found {GuildId} {AnnouncementRoleId}", guildId, announcementRoleId);
            return Result.Fail<IRole>("Announcement role not found");
        }
        catch (Exception e)
        {
            LogErrorException(e);
            return Result.Fail<IRole>(e.Message);
        }
    }

    [LoggerMessage(EventId = 0, Level = LogLevel.Error, Message = "Exception occurred in GuildOptionsService")]
    public partial void LogErrorException(Exception ex);
}
