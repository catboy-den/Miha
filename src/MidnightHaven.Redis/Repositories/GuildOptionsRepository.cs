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

    public async Task<GuildOptions?> GetAsync(ulong? guildId)
    {
        var id = guildId.ToString() ?? string.Empty;

        id.Should().NotBeNullOrEmpty();

        var collection = _provider.RedisCollection<GuildOptions>();

        return await collection.FindByIdAsync(id);
    }

    public async Task<GuildOptions?> UpsertAsync(GuildOptions options)
    {
        options.Should().NotBeNull();

        var collection = _provider.RedisCollection<GuildOptions>();
        var exists = await collection.FindByIdAsync(options.GuildId.ToString()) is not null;

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

        var collection = _provider.RedisCollection<GuildOptions>();
        var options = await collection.FindByIdAsync(guildId.ToString()!);

        options.Should().NotBeNull();

        await collection.DeleteAsync(options!);
    }
}
