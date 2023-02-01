using Discord;
using FluentResults;
using MidnightHaven.Redis.Models;

namespace MidnightHaven.Chan.Services.Logic.Interfaces;

public interface IGuildOptionsService
{
    Task<Result<GuildSettings?>> GetAsync(ulong? guildId);
    Task<Result<GuildSettings?>> UpsertAsync(GuildSettings settings);
    Task<Result<GuildSettings?>> UpsertAsync(ulong? guildId, Action<GuildSettings> optionsFunc);
    Task<Result> DeleteAsync(ulong? guildId);

    Task<Result<ITextChannel>> GetLoggingChannelAsync(ulong? guildId);
    Task<Result<ITextChannel>> GetAnnouncementChannelAsync(ulong? guildId);
    Task<Result<IRole>> GetAnnouncementRoleAsync(ulong? guildId);
}
