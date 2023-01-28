using FluentResults;
using MidnightHaven.Redis.Models;

namespace MidnightHaven.Redis.Services.Interfaces;

public interface IGuildOptionsService
{
    Task<Result<GuildOptions?>> GetAsync(ulong? guildId);
    Task<Result<GuildOptions?>> UpsertAsync(GuildOptions options);
    Task<Result> DeleteAsync(ulong? guildId);
}
