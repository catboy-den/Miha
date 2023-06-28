using Miha.Redis.Documents;
using Miha.Redis.Repositories.Interfaces;
using Redis.OM.Contracts;

namespace Miha.Redis.Repositories;

public class BirthdayJobRepository : DocumentRepository<BirthdayJobDocument>, IBirthdayJobRepository
{
    public BirthdayJobRepository(IRedisConnectionProvider provider) : base(provider)
    {

    }
}
