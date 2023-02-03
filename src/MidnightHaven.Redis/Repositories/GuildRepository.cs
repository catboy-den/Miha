using FluentAssertions;
using MidnightHaven.Redis.Documents;
using MidnightHaven.Redis.Repositories.Interfaces;
using Redis.OM.Contracts;

namespace MidnightHaven.Redis.Repositories;

public class GuildRepository : IGuildRepository
{
    private readonly IRedisConnectionProvider _provider;

    public GuildRepository(IRedisConnectionProvider provider)
    {
        _provider = provider;
    }

    public async Task<GuildDocument?> GetAsync(ulong? guildId)
    {
        var id = guildId.ToString() ?? string.Empty;

        id.Should().NotBeNullOrEmpty();

        var collection = _provider.RedisCollection<GuildDocument>();

        return await collection.FindByIdAsync(id);
    }

    public Task<GuildDocument?> UpsertAsync(GuildDocument document)
    {
        return UpsertAsync(document.GuildId, guildDoc => guildDoc = document);
    }

    public async Task<GuildDocument?> UpsertAsync(ulong? guildId, Action<GuildDocument> documentFunc)
    {
        var id = guildId.ToString() ?? string.Empty;

        id.Should().NotBeNullOrEmpty();

        var collection = _provider.RedisCollection<GuildDocument>();

        var options = await collection.FindByIdAsync(id);
        var exists = options != null;

        options ??= new GuildDocument { GuildId = guildId!.Value };

        documentFunc(options);

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

        var collection = _provider.RedisCollection<GuildDocument>();
        var options = await collection.FindByIdAsync(guildId.ToString()!);

        options.Should().NotBeNull();

        await collection.DeleteAsync(options!);
    }
}
