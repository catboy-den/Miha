using FluentResults;
using Microsoft.Extensions.Logging;
using MidnightHaven.Redis.Documents;
using MidnightHaven.Redis.Repositories.Interfaces;
using NodaTime;
using NodaTime.Calendars;
using Redis.OM;
using Redis.OM.Aggregation.AggregationPredicates;
using Redis.OM.Contracts;

namespace MidnightHaven.Redis.Repositories;

public partial class UserRepository : DocumentRepository<UserDocument>, IUserRepository
{
    private readonly IRedisConnectionProvider _provider;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(
        IRedisConnectionProvider provider,
        ILogger<UserRepository> logger) : base(provider)
    {
        _provider = provider;
        _logger = logger;
    }

    public async Task<IEnumerable<UserDocument>> GetAllUsersWithBirthdateInWeekAsync(LocalDate weekDate)
    {
        var collection = _provider.RedisCollection<UserDocument>();

        var weekNumberInYear = WeekYearRules.Iso.GetWeekOfWeekYear(weekDate);
        var usersWithBirthdate = await collection.Where(user => user.EnableBirthday).ToListAsync();

        return usersWithBirthdate.Where(user =>
            user.AnnualBirthdate.HasValue
            && WeekYearRules.Iso.GetWeekOfWeekYear(new LocalDate(weekDate.Year, user.AnnualBirthdate!.Value.Month, user.AnnualBirthdate.Value.Day)) == weekNumberInYear);
    }

    [LoggerMessage(EventId = 0, Level = LogLevel.Error, Message = "Exception caught")]
    public partial void LogErrorException(Exception ex);
}
