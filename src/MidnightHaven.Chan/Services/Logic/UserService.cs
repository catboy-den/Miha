using System.Text.RegularExpressions;
using FluentResults;
using Microsoft.Extensions.Logging;
using MidnightHaven.Chan.Services.Logic.Interfaces;
using MidnightHaven.Redis.Documents;
using MidnightHaven.Redis.Repositories.Interfaces;

namespace MidnightHaven.Chan.Services.Logic;

public partial class UserService : DocumentService<UserDocument>, IUserService
{
    private readonly IUserRepository _repository;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository repository,
        ILogger<UserService> logger) : base(repository, logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<UserDocument?>> UpsertVrcUsrIdAsync(ulong? userId, string vrcProfileUrl)
    {
        try
        {
            var usrId = UsrRegex().Match(vrcProfileUrl);

            if (!usrId.Success || string.IsNullOrEmpty(usrId.Value))
            {
                return Result.Fail<UserDocument?>("Couldn't find the usr_Id in the passed link");
            }

            return await _repository.UpsertAsync(userId, doc => doc.VrcUsrId = usrId.Value);
        }
        catch (Exception e)
        {
            LogErrorException(e);
            return Result.Fail(e.Message);
        }
    }

    public async Task<Result<UserDocument?>> ClearVrcUsrIdAsync(ulong? userId)
    {
        try
        {
            return await _repository.UpsertAsync(userId, doc => doc.VrcUsrId = null);
        }
        catch (Exception e)
        {
            LogErrorException(e);
            return Result.Fail(e.Message);
        }
    }

    [GeneratedRegex("usr_[a-fA-F0-9]{8}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{12}")]
    private static partial Regex UsrRegex();
}
