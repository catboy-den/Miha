using FluentResults;
using Microsoft.Extensions.Logging;
using MidnightHaven.Chan.Services.Logic.Interfaces;
using MidnightHaven.Redis.Repositories.Interfaces;

namespace MidnightHaven.Chan.Services.Logic;

public partial class DocumentService<T> : IDocumentService<T>
{
    private readonly IDocumentRepository<T> _repository;
    private readonly ILogger _logger;

    protected DocumentService(
        IDocumentRepository<T> repository,
        ILogger logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<T?>> GetAsync(ulong? documentId)
    {
        try
        {
            if (documentId is null)
            {
                throw new ArgumentNullException(nameof(documentId));
            }

            return Result.Ok(await _repository.GetAsync(documentId));
        }
        catch (Exception e)
        {
            LogErrorException(e);
            return Result.Fail(e.Message);
        }
    }

    public async Task<Result<T?>> UpsertAsync(T document)
    {
        try
        {
            return Result.Ok(await _repository.UpsertAsync(document));
        }
        catch (Exception e)
        {
            LogErrorException(e);
            return Result.Fail(e.Message);
        }
    }

    public async Task<Result<T?>> UpsertAsync(ulong? documentId, Action<T> documentFunc)
    {
        try
        {
            if (documentId is null)
            {
                throw new ArgumentNullException(nameof(documentId));
            }

            return Result.Ok(await _repository.UpsertAsync(documentId, documentFunc));
        }
        catch (Exception e)
        {
            LogErrorException(e);
            return Result.Fail(e.Message);
        }
    }

    public async Task<Result> DeleteAsync(ulong? documentId)
    {
        try
        {
            if (documentId is null)
            {
                throw new ArgumentNullException(nameof(documentId));
            }

            await _repository.DeleteAsync(documentId);
            return Result.Ok();
        }
        catch (Exception e)
        {
            LogErrorException(e);
            return Result.Fail(e.Message);
        }
    }

    [LoggerMessage(EventId = 0, Level = LogLevel.Error, Message = "Exception caught")]
    public partial void LogErrorException(Exception ex);
}
