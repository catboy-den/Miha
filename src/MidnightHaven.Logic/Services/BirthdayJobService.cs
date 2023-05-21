using Microsoft.Extensions.Logging;
using MidnightHaven.Logic.Services.Interfaces;
using MidnightHaven.Redis.Documents;
using MidnightHaven.Redis.Repositories.Interfaces;

namespace MidnightHaven.Logic.Services;

public class BirthdayJobService : DocumentService<BirthdayJobDocument>, IBirthdayJobService
{
    public BirthdayJobService(
        IDocumentRepository<BirthdayJobDocument> repository,
        ILogger<BirthdayJobService> logger) : base(repository, logger)
    {

    }
}
