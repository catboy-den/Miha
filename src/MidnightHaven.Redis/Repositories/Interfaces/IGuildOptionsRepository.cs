using MidnightHaven.Redis.Models;

namespace MidnightHaven.Redis.Repositories.Interfaces;

public interface IGuildOptionsRepository
{
    Task<GuildSettings?> GetAsync(ulong? guildId);

    Task<GuildSettings?> UpsertAsync(GuildSettings settings);
    Task<GuildSettings?> UpsertAsync(ulong? guildId, Action<GuildSettings> optionsFunc);

    Task DeleteAsync(ulong? guildId);
}
