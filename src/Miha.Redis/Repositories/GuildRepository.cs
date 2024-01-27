using Miha.Redis.Documents;
using Miha.Redis.Repositories.Interfaces;
using Redis.OM.Contracts;

namespace Miha.Redis.Repositories;

public class GuildRepository(IRedisConnectionProvider provider) : DocumentRepository<GuildDocument>(provider), IGuildRepository;
