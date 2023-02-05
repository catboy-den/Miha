using MidnightHaven.Redis.Documents;
using MidnightHaven.Redis.Repositories.Interfaces;
using Redis.OM.Contracts;

namespace MidnightHaven.Redis.Repositories;

public class UserRepository : DocumentRepository<UserDocument>, IUserRepository
{
    public UserRepository(IRedisConnectionProvider provider) : base(provider)
    {
    }
}
