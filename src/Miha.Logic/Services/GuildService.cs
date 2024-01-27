using Discord;
using Discord.WebSocket;
using FluentResults;
using Microsoft.Extensions.Logging;
using Miha.Logic.Services.Interfaces;
using Miha.Redis.Documents;
using Miha.Redis.Repositories.Interfaces;

namespace Miha.Logic.Services;

public class GuildService(
    DiscordSocketClient client,
    IGuildRepository repository,
    ILogger<GuildService> logger) : DocumentService<GuildDocument>(repository, logger), IGuildService
{
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
                logger.LogWarning("Guild doesn't have any document when trying to get logging channel {GuildId}", guildId);
                return optionsResult.ToResult<ITextChannel>();
            }

            var logChannel = optionsResult.Value?.LogChannel;
            if (!logChannel.HasValue)
            {
                logger.LogDebug("Guild doesn't have a logging channel set {GuildId}", guildId);
                return Result.Fail<ITextChannel>("Logging channel not set");
            }

            if (await client.GetChannelAsync(logChannel.Value) is ITextChannel loggingChannel)
            {
                return Result.Ok(loggingChannel);
            }

            logger.LogWarning("Guild's logging channel wasn't found, or might not be a Text Channel {GuildId} {LoggingChannelId}", guildId, logChannel.Value);
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
                logger.LogWarning("Guild doesn't have any document when trying to get announcement channel {GuildId}", guildId);
                return optionsResult.ToResult<ITextChannel>();
            }

            var announcementChannel = optionsResult.Value?.AnnouncementChannel;
            if (!announcementChannel.HasValue)
            {
                logger.LogDebug("Guild doesn't have a announcement channel set {GuildId}", guildId);
                return Result.Fail<ITextChannel>("Announcement channel not set");
            }

            if (await client.GetChannelAsync(announcementChannel.Value) is ITextChannel loggingChannel)
            {
                return Result.Ok(loggingChannel);
            }

            logger.LogWarning("Guild's announcement channel wasn't found, or might not be a Text Channel {GuildId} {AnnouncementChannelId}", guildId, announcementChannel.Value);
            return Result.Fail<ITextChannel>("Announcement channel not found");
        }
        catch (Exception e)
        {
            LogErrorException(e);
            return Result.Fail<ITextChannel>(e.Message);
        }
    }

    public async Task<Result<ITextChannel>> GetWeeklyScheduleChannel(ulong? guildId)
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
                logger.LogWarning("Guild doesn't have any document when trying to get announcement channel {GuildId}", guildId);
                return optionsResult.ToResult<ITextChannel>();
            }

            var weeklyScheduleChannel = optionsResult.Value?.WeeklyScheduleChannel;
            if (!weeklyScheduleChannel.HasValue)
            {
                logger.LogDebug("Guild doesn't have a weekly schedule channel set {GuildId}", guildId);
                return Result.Fail<ITextChannel>("Weekly schedule channel not set");
            }

            if (await client.GetChannelAsync(weeklyScheduleChannel.Value) is ITextChannel channel)
            {
                return Result.Ok(channel);
            }

            logger.LogWarning("Guild's weekly schedule channel wasn't found, or might not be a Text Channel {GuildId} {WeeklyScheduleChannel}", guildId, weeklyScheduleChannel.Value);
            return Result.Fail<ITextChannel>("Weekly schedule channel not found");
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
                logger.LogWarning("Guild doesn't have any document when trying to get announcement role {GuildId}", guildId);
                return optionsResult.ToResult<IRole>();
            }

            var announcementRoleId = optionsResult.Value?.AnnouncementRoleId;
            if (!announcementRoleId.HasValue)
            {
                logger.LogDebug("Guild doesn't have a announcement role set {GuildId}", guildId);
                return Result.Fail<IRole>("Announcement role not set");
            }

            if (client.GetGuild(guildId.Value).GetRole(announcementRoleId.Value) is IRole announcementRole)
            {
                return Result.Ok(announcementRole);
            }

            logger.LogWarning("Guild's announcement role wasn't found {GuildId} {AnnouncementRoleId}", guildId, announcementRoleId);
            return Result.Fail<IRole>("Announcement role not found");
        }
        catch (Exception e)
        {
            LogErrorException(e);
            return Result.Fail<IRole>(e.Message);
        }
    }
}
