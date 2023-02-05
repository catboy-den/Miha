using MidnightHaven.Redis.Documents;
using MidnightHaven.Redis.Repositories.Interfaces;
using Redis.OM.Contracts;

namespace MidnightHaven.Redis.Repositories;

public class GuildRepository : DocumentRepository<GuildDocument>, IGuildRepository
{
    public GuildRepository(IRedisConnectionProvider provider) : base(provider)
    {

    }
}
