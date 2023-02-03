using MidnightHaven.Redis.Documents;

namespace MidnightHaven.Redis.Repositories.Interfaces;

public interface IGuildRepository
{
    Task<GuildDocument?> GetAsync(ulong? guildId);

    Task<GuildDocument?> UpsertAsync(GuildDocument document);
    Task<GuildDocument?> UpsertAsync(ulong? guildId, Action<GuildDocument> documentFunc);

    Task DeleteAsync(ulong? guildId);
}
