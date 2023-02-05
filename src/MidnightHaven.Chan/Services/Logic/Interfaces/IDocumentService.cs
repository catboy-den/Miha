using FluentResults;

namespace MidnightHaven.Chan.Services.Logic.Interfaces;

public interface IDocumentService<T>
{
    Task<Result<T?>> GetAsync(ulong? documentId);
    Task<Result<T?>> UpsertAsync(T document);
    Task<Result<T?>> UpsertAsync(ulong? documentId, Action<T> documentFunc);
    Task<Result> DeleteAsync(ulong? documentId);
}
