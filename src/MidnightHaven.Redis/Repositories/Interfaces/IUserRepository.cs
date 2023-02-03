using MidnightHaven.Redis.Documents;

namespace MidnightHaven.Redis.Repositories.Interfaces;

public interface IUserRepository
{
    Task<UserDocument?> GetAsync(ulong? userId);

    Task<UserDocument?> UpsertAsync(UserDocument document);
    Task<UserDocument?> UpsertAsync(ulong? userId, Action<UserDocument> optionsFunc);

    Task DeleteAsync(ulong? guildId);
}
