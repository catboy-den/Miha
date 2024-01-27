using Microsoft.Extensions.Logging;
using Miha.Logic.Services.Interfaces;
using Miha.Redis.Documents;
using Miha.Redis.Repositories.Interfaces;

namespace Miha.Logic.Services;

public class BirthdayJobService(
    IBirthdayJobRepository repository,
    ILogger<BirthdayJobService> logger) : DocumentService<BirthdayJobDocument>(repository, logger), IBirthdayJobService;
