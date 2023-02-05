using FluentResults;
using Microsoft.Extensions.Logging;
using MidnightHaven.Chan.Services.Logic.Interfaces;
using MidnightHaven.Redis.Documents;
using MidnightHaven.Redis.Repositories.Interfaces;

namespace MidnightHaven.Chan.Services.Logic;

public partial class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository repository,
        ILogger<UserService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<UserDocument?>> GetAsync(ulong? userId)
    {
        try
        {
            if (userId is null)
            {
                throw new ArgumentNullException(nameof(userId));
            }

            return Result.Ok(await _repository.GetAsync(userId));
        }
        catch (Exception e)
        {
            LogErrorException(e);
            return Result.Fail(e.Message);
        }
    }

    public async Task<Result<UserDocument?>> UpsertAsync(UserDocument document)
    {
        try
        {
            return Result.Ok(await _repository.UpsertAsync(document));
        }
        catch (Exception e)
        {
            LogErrorException(e);
            return Result.Fail(e.Message);
        }
    }

    public async Task<Result<UserDocument?>> UpsertAsync(ulong? userId, Action<UserDocument> documentFunc)
    {
        try
        {
            if (userId is null)
            {
                throw new ArgumentNullException(nameof(userId));
            }

            return Result.Ok(await _repository.UpsertAsync(userId, documentFunc));
        }
        catch (Exception e)
        {
            LogErrorException(e);
            return Result.Fail(e.Message);
        }
    }

    public async Task<Result> DeleteAsync(ulong? userId)
    {
        try
        {
            if (userId is null)
            {
                throw new ArgumentNullException(nameof(userId));
            }

            await _repository.DeleteAsync(userId);
            return Result.Ok();
        }
        catch (Exception e)
        {
            LogErrorException(e);
            return Result.Fail(e.Message);
        }
    }

    [LoggerMessage(EventId = 0, Level = LogLevel.Error, Message = "Exception occurred in GuildService")]
    public partial void LogErrorException(Exception ex);
}
