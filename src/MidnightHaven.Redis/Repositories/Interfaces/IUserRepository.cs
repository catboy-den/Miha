using MidnightHaven.Redis.Documents;
using NodaTime;

namespace MidnightHaven.Redis.Repositories.Interfaces;

public interface IUserRepository : IDocumentRepository<UserDocument>
{
    Task<IEnumerable<UserDocument>> GetAllUsersWithBirthdateInWeekAsync(LocalDate weekDate);
}
