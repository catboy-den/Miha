using FluentResults;

namespace Miha.Logic.Services.Interfaces;

public interface IDocumentService<T>
{
    Task<Result<IEnumerable<T>>> GetAllAsync();

    Task<Result<T?>> GetAsync(ulong? documentId);
    Task<Result<T?>> UpsertAsync(T document);
    Task<Result<T?>> UpsertAsync(ulong? documentId, Action<T> documentFunc);
    Task<Result> DeleteAsync(ulong? documentId, bool successIfNotFound = false);
}
