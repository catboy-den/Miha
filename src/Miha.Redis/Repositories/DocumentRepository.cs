using FluentAssertions;
using Miha.Redis.Documents;
using Miha.Redis.Repositories.Interfaces;
using Redis.OM.Contracts;

namespace Miha.Redis.Repositories;

public class DocumentRepository<T>(IRedisConnectionProvider provider) : IDocumentRepository<T> where T : Document, new()
{
    public async Task<IEnumerable<T>> GetAllAsync()
    {
        var collection = provider.RedisCollection<T>();

        return await collection.ToListAsync();
    }

    public async Task<T?> GetAsync(ulong? documentId)
    {
        var id = documentId.ToString() ?? string.Empty;

        id.Should().NotBeNullOrEmpty();

        var collection = provider.RedisCollection<T>();

        return await collection.FindByIdAsync(id);
    }

    public async Task<T?> UpsertAsync(T document)
    {
        var id = document.Id.ToString();
        id.Should().NotBeNullOrEmpty();

        var collection = provider.RedisCollection<T>();
        var doc = await collection.FindByIdAsync(id);
        var exists = doc is not null;

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

    public async Task<T?> UpsertAsync(ulong? documentId, Action<T> documentFunc)
    {
        documentId.Should().HaveValue();

        var id = documentId.ToString() ?? string.Empty;
        id.Should().NotBeNullOrEmpty();

        var collection = provider.RedisCollection<T>();
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

    public async Task DeleteAsync(ulong? documentId, bool successIfNotFound = false)
    {
        documentId.Should().NotBeNull();

        var collection = provider.RedisCollection<T>();
        var document = await collection.FindByIdAsync(documentId.ToString()!);

        if (successIfNotFound && document is null)
        {
            return;
        }

        document.Should().NotBeNull();

        await collection.DeleteAsync(document!);
    }
}
