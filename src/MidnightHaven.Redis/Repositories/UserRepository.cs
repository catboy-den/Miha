using Microsoft.Extensions.Logging;
using MidnightHaven.Redis.Documents;
using MidnightHaven.Redis.Repositories.Interfaces;
using Redis.OM;
using Redis.OM.Contracts;

namespace MidnightHaven.Redis.Repositories;

public partial class UserRepository : DocumentRepository<UserDocument>, IUserRepository
{
    private readonly IRedisConnectionProvider _provider;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(
        IRedisConnectionProvider provider,
        ILogger<UserRepository> logger) : base(provider)
    {
        _provider = provider;
        _logger = logger;
    }

    public async Task<IEnumerable<UserDocument>> GetAllUsersWithBirthdayEnabledAsync()
    {
        var collection = _provider.RedisCollection<UserDocument>();

        return await collection.Where(user => user.EnableBirthday).ToListAsync();
    }

    [LoggerMessage(EventId = 0, Level = LogLevel.Error, Message = "Exception caught")]
    public partial void LogErrorException(Exception ex);
}
