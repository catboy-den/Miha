using MidnightHaven.Redis.Models;

namespace MidnightHaven.Redis.Repositories.Interfaces;

public interface IGuildOptionsRepository
{
    Task<GuildOptions?> GetAsync(ulong? guildId);

    Task<GuildOptions?> UpsertAsync(GuildOptions options);

    Task DeleteAsync(ulong? guildId);
}
