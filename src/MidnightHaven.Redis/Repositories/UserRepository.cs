using FluentAssertions;
using MidnightHaven.Redis.Documents;
using MidnightHaven.Redis.Repositories.Interfaces;
using Redis.OM.Contracts;

namespace MidnightHaven.Redis.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IRedisConnectionProvider _provider;

    public UserRepository(IRedisConnectionProvider provider)
    {
        _provider = provider;
    }

    public async Task<UserDocument?> GetAsync(ulong? guildId)
    {
        var id = guildId.ToString() ?? string.Empty;

        id.Should().NotBeNullOrEmpty();

        var collection = _provider.RedisCollection<UserDocument>();

        return await collection.FindByIdAsync(id);
    }

    public Task<UserDocument?> UpsertAsync(UserDocument document)
    {
        return UpsertAsync(document.UserId, guildDoc => guildDoc = document);
    }

    public async Task<UserDocument?> UpsertAsync(ulong? guildId, Action<UserDocument> documentFunc)
    {
        var id = guildId.ToString() ?? string.Empty;

        id.Should().NotBeNullOrEmpty();

        var collection = _provider.RedisCollection<UserDocument>();

        var options = await collection.FindByIdAsync(id);
        var exists = options != null;

        options ??= new UserDocument { UserId = guildId!.Value };

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

        var collection = _provider.RedisCollection<UserDocument>();
        var options = await collection.FindByIdAsync(guildId.ToString()!);

        options.Should().NotBeNull();

        await collection.DeleteAsync(options!);
    }
}
