using Miha.Redis.Documents;
using Miha.Redis.Repositories.Interfaces;
using Redis.OM.Contracts;

namespace Miha.Redis.Repositories;

public class GuildRepository : DocumentRepository<GuildDocument>, IGuildRepository
{
    public GuildRepository(IRedisConnectionProvider provider) : base(provider)
    {

    }
}
