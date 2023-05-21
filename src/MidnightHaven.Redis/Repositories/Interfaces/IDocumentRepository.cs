namespace MidnightHaven.Redis.Repositories.Interfaces;

public interface IDocumentRepository<T>
{
    Task<IEnumerable<T>> GetAllAsync();

    Task<T?> GetAsync(ulong? documentId);

    Task<T?> UpsertAsync(T documentId);
    Task<T?> UpsertAsync(ulong? userId, Action<T> optionsFunc);

    Task DeleteAsync(ulong? documentId, bool successIfNotFound = false);
}
