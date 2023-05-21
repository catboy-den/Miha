using FluentResults;
using MidnightHaven.Redis.Documents;
using NodaTime;

namespace MidnightHaven.Logic.Services.Interfaces;

public interface IUserService
{
    Task<Result<UserDocument?>> GetAsync(ulong? userId);
    Task<Result<UserDocument?>> UpsertAsync(UserDocument document);
    Task<Result<UserDocument?>> UpsertAsync(ulong? userId, Action<UserDocument> userFunc);
    Task<Result> DeleteAsync(ulong? userId);

    Task<Result<IEnumerable<UserDocument>>> GetAllUsersWithBirthdayForWeekAsync(LocalDate weekDate, bool includeAlreadyAnnounced);
    Task<Result<UserDocument?>> UpsertVrchatUserIdAsync(ulong? userId, string vrcProfileUrl);
}
