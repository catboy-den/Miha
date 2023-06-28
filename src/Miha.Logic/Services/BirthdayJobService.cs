using Microsoft.Extensions.Logging;
using Miha.Logic.Services.Interfaces;
using Miha.Redis.Documents;
using Miha.Redis.Repositories.Interfaces;

namespace Miha.Logic.Services;

public class BirthdayJobService : DocumentService<BirthdayJobDocument>, IBirthdayJobService
{
    public BirthdayJobService(
        IBirthdayJobRepository repository,
        ILogger<BirthdayJobService> logger) : base(repository, logger)
    {

    }
}
