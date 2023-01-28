using FluentResults;
using Microsoft.Extensions.Logging;
using MidnightHaven.Redis.Models;
using MidnightHaven.Redis.Repositories.Interfaces;
using MidnightHaven.Redis.Services.Interfaces;

namespace MidnightHaven.Redis.Services;

public partial class GuildOptionsService : IGuildOptionsService
{
    private readonly IGuildOptionsRepository _repository;
    private readonly ILogger<GuildOptionsService> _logger;

    public GuildOptionsService(
        IGuildOptionsRepository repository,
        ILogger<GuildOptionsService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<GuildOptions?>> GetAsync(ulong? guildId)
    {
        try
        {
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

    public async Task<Result> DeleteAsync(ulong? guildId)
    {
        try
        {
            await _repository.DeleteAsync(guildId);
            return Result.Ok();
        }
        catch (Exception e)
        {
            LogErrorException(e);
            return Result.Fail(e.Message);
        }
    }

    [LoggerMessage(EventId = 0, Level = LogLevel.Error, Message = "Exception occurred in GuildOptionsService")]
    public partial void LogErrorException(Exception ex);
}
