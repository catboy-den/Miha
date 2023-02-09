using FluentAssertions;
using MidnightHaven.Redis.Documents;
using MidnightHaven.Redis.Repositories.Interfaces;
using Redis.OM.Contracts;

namespace MidnightHaven.Redis.Repositories;

public class DocumentRepository<T> : IDocumentRepository<T> where T : Document, new()
{
    private readonly IRedisConnectionProvider _provider;

    public DocumentRepository(IRedisConnectionProvider provider)
    {
        _provider = provider;
    }

    public async Task<T?> GetAsync(ulong? documentId)
    {
        var id = documentId.ToString() ?? string.Empty;

        id.Should().NotBeNullOrEmpty();

        var collection = _provider.RedisCollection<T>();

        return await collection.FindByIdAsync(id);
    }

    public Task<T?> UpsertAsync(T document)
    {
        return UpsertAsync(document.Id, doc => doc = document);
    }

    public async Task<T?> UpsertAsync(ulong? documentId, Action<T> documentFunc)
    {
        documentId.Should().HaveValue();

        var id = documentId.ToString() ?? string.Empty;
        id.Should().NotBeNullOrEmpty();

        var collection = _provider.RedisCollection<T>();
        var document = await collection.FindByIdAsync(id);
        var exists = document is not null;

        document ??= new T { Id = documentId!.Value };

        documentFunc(document);

        if (exists)
        {
            await collection.UpdateAsync(document);
        }
        else
        {
            await collection.InsertAsync(document);
        }

        return document;
    }

    public async Task DeleteAsync(ulong? documentId)
    {
        documentId.Should().NotBeNull();

        var collection = _provider.RedisCollection<T>();
        var document = await collection.FindByIdAsync(documentId.ToString()!);

        document.Should().NotBeNull();

        await collection.DeleteAsync(document!);
    }
}
