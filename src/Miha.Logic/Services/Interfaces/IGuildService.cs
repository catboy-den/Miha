using Discord;
using FluentResults;
using Miha.Redis.Documents;

namespace Miha.Logic.Services.Interfaces;

public interface IGuildService
{
    Task<Result<GuildDocument?>> GetAsync(ulong? guildId);
    Task<Result<GuildDocument?>> UpsertAsync(GuildDocument document);
    Task<Result<GuildDocument?>> UpsertAsync(ulong? guildId, Action<GuildDocument> documentFunc);
    Task<Result> DeleteAsync(ulong? guildId, bool successIfNotFound = false);

    Task<Result<ITextChannel>> GetLoggingChannelAsync(ulong? guildId);
    Task<Result<ITextChannel>> GetAnnouncementChannelAsync(ulong? guildId);
    Task<Result<IRole>> GetAnnouncementRoleAsync(ulong? guildId);
}
