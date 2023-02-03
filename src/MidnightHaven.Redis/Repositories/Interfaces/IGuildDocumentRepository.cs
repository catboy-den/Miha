using MidnightHaven.Redis.Models;

namespace MidnightHaven.Redis.Repositories.Interfaces;

public interface IGuildDocumentRepository
{
    Task<GuildDocument?> GetAsync(ulong? guildId);

    Task<GuildDocument?> UpsertAsync(GuildDocument document);
    Task<GuildDocument?> UpsertAsync(ulong? guildId, Action<GuildDocument> optionsFunc);

    Task DeleteAsync(ulong? guildId);
}
