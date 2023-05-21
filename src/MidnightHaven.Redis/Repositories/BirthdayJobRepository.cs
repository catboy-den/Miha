using MidnightHaven.Redis.Documents;
using MidnightHaven.Redis.Repositories.Interfaces;
using Redis.OM.Contracts;

namespace MidnightHaven.Redis.Repositories;

public class BirthdayJobRepository : DocumentRepository<BirthdayJobDocument>, IBirthdayJobRepository
{
    public BirthdayJobRepository(IRedisConnectionProvider provider) : base(provider)
    {

    }
}
