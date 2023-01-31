using Discord;
using FluentResults;
using MidnightHaven.Redis.Models;

namespace MidnightHaven.Chan.Services.Logic.Interfaces;

public interface IGuildOptionsService
{
    Task<Result<GuildOptions?>> GetAsync(ulong? guildId);
    Task<Result<GuildOptions?>> UpsertAsync(GuildOptions options);
    Task<Result<GuildOptions?>> UpsertAsync(ulong? guildId, Action<GuildOptions> optionsFunc);
    Task<Result> DeleteAsync(ulong? guildId);

    Task<Result<ITextChannel>> GetLoggingChannelAsync(ulong? guildId);
    Task<Result<ITextChannel>> GetAnnouncementChannelAsync(ulong? guildId);
    Task<Result<IRole>> GetAnnouncementRoleAsync(ulong? guildId);
}
