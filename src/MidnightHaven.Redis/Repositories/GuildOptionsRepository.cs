using FluentAssertions;
using MidnightHaven.Redis.Models;
using MidnightHaven.Redis.Repositories.Interfaces;
using Redis.OM.Contracts;

namespace MidnightHaven.Redis.Repositories;

public class GuildOptionsRepository : IGuildOptionsRepository
{
    private readonly IRedisConnectionProvider _provider;

    public GuildOptionsRepository(IRedisConnectionProvider provider)
    {
        _provider = provider;
    }

    public async Task<GuildSettings?> GetAsync(ulong? guildId)
    {
        var id = guildId.ToString() ?? string.Empty;

        id.Should().NotBeNullOrEmpty();

        var collection = _provider.RedisCollection<GuildSettings>();

        return await collection.FindByIdAsync(id);
    }

    public Task<GuildSettings?> UpsertAsync(GuildSettings settings)
    {
        return UpsertAsync(settings.GuildId, guildOptions => guildOptions = settings);
    }

    public async Task<GuildSettings?> UpsertAsync(ulong? guildId, Action<GuildSettings> optionsFunc)
    {
        var id = guildId.ToString() ?? string.Empty;

        id.Should().NotBeNullOrEmpty();

        var collection = _provider.RedisCollection<GuildSettings>();

        var options = await collection.FindByIdAsync(id);
        var exists = options != null;

        options ??= new GuildSettings { GuildId = guildId!.Value };

        optionsFunc(options);

        if (exists)
        {
            await collection.UpdateAsync(options);
        }
        else
        {
            await collection.InsertAsync(options);
        }

        return options;
    }

    public async Task DeleteAsync(ulong? guildId)
    {
        guildId.Should().NotBeNull();

        var collection = _provider.RedisCollection<GuildSettings>();
        var options = await collection.FindByIdAsync(guildId.ToString()!);

        options.Should().NotBeNull();

        await collection.DeleteAsync(options!);
    }
}
