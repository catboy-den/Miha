using System.Text.RegularExpressions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Miha.Logic.Services.Interfaces;
using Miha.Redis.Documents;
using Miha.Redis.Repositories.Interfaces;
using NodaTime;
using NodaTime.Calendars;

namespace Miha.Logic.Services;

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

    public async Task<Result<IEnumerable<UserDocument>>> GetAllUsersWithBirthdayForWeekAsync(LocalDate weekDate, bool includeAlreadyAnnounced)
    {
        var weekNumberInYear = WeekYearRules.Iso.GetWeekOfWeekYear(weekDate);
        var usersWithBirthday = await _repository.GetAllUsersWithBirthdayEnabledAsync();

        // Add a method to User document which will return the localDate or AnnualDate in EST of their birthday

        if (includeAlreadyAnnounced)
        {
            return Result.Ok(usersWithBirthday.Where(user =>
                // the users birthday falls in the same week as the weekDate
                user is { AnnualBirthdate: not null, Timezone: not null }
                && WeekYearRules.Iso.GetWeekOfWeekYear(user.GetBirthdateInEst(weekDate.Year)!.Value) == weekNumberInYear));
        }

        return Result.Ok(usersWithBirthday.Where(user =>
            // the users birthday falls in the same week as the weekDate
            user is { AnnualBirthdate: not null, Timezone: not null }
            && WeekYearRules.Iso.GetWeekOfWeekYear(user.GetBirthdateInEst(weekDate.Year)!.Value) == weekNumberInYear
            // the user doesn't have any last announcement OR the last announcement year is before the weekDate year
            && (!user.LastBirthdateAnnouncement.HasValue || user.LastBirthdateAnnouncement.Value.Year < weekDate.Year)));
    }

    public async Task<Result<UserDocument?>> UpsertVrchatUserIdAsync(ulong? userId, string vrcProfileUrl)
    {
        try
        {
            var usrId = UsrRegex().Match(vrcProfileUrl);

            if (!usrId.Success || string.IsNullOrEmpty(usrId.Value))
            {
                return Result.Fail<UserDocument?>("Couldn't find the usr_Id in the passed link");
            }

            return await _repository.UpsertAsync(userId, doc => doc.VrcUserId = usrId.Value);
        }
        catch (Exception e)
        {
            LogErrorException(e);
            return Result.Fail(e.Message);
        }
    }

    [GeneratedRegex("usr_[a-fA-F0-9]{8}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{12}")]
    private static partial Regex UsrRegex();
}
