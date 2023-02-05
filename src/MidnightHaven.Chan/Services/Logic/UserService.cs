using Microsoft.Extensions.Logging;
using MidnightHaven.Chan.Services.Logic.Interfaces;
using MidnightHaven.Redis.Documents;
using MidnightHaven.Redis.Repositories.Interfaces;

namespace MidnightHaven.Chan.Services.Logic;

public partial class UserService : DocumentService<UserDocument>, IUserService
{
    private readonly IUserRepository _repository;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository repository,
        ILogger<UserService> logger) : base(repository, logger)
    {
        _repository = repository;
        _logger = logger;
    }
}
