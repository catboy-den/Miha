using Miha.Redis.Documents;

namespace Miha.Redis.Repositories.Interfaces;

public interface IUserRepository : IDocumentRepository<UserDocument>
{
    Task<IEnumerable<UserDocument>> GetAllUsersWithBirthdayEnabledAsync();
}
