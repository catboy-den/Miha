using Cronos;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Miha.Logic.Services.Interfaces;
using Miha.Redis.Documents;
using Miha.Shared.ZonedClocks.Interfaces;

namespace Miha.Services;

/// <summary>
/// Scans birthdays, and creates BirthdayJobDocuments
/// </summary>
public class BirthdayScannerService : BackgroundService
{
    private const string Schedule = "*/5 8-19 * * *"; // https://crontab.cronhub.io/

    private readonly IEasternStandardZonedClock _easternStandardZonedClock;
    private readonly IUserService _userService;
    private readonly IBirthdayJobService _birthdayJobService;
    private readonly ILogger<BirthdayScannerService> _logger;

    private readonly CronExpression _cron;

    public BirthdayScannerService(
        IEasternStandardZonedClock easternStandardZonedClock,
        IUserService userService,
        IBirthdayJobService birthdayJobService,
        ILogger<BirthdayScannerService> logger)
    {
        _easternStandardZonedClock = easternStandardZonedClock;
        _userService = userService;
        _birthdayJobService = birthdayJobService;
        _logger = logger;

        _cron = CronExpression.Parse(Schedule, CronFormat.Standard);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await ScanBirthdaysAsync();

            var utcNow = _easternStandardZonedClock.GetCurrentInstant().ToDateTimeUtc();
            var nextUtc = _cron.GetNextOccurrence(DateTimeOffset.UtcNow, _easternStandardZonedClock.GetTimeZoneInfo());

            if (nextUtc is null)
            {
                _logger.LogWarning("Next utc occurence is null");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                continue;
            }

            _logger.LogDebug("Waiting {Time}", nextUtc.Value - utcNow);
            await Task.Delay(nextUtc.Value - utcNow, stoppingToken);
        }
    }

    private async Task ScanBirthdaysAsync()
    {
        _logger.LogInformation("Scanning for birthdays");

        var today = _easternStandardZonedClock.GetCurrentDate();
        var unannouncedBirthdaysThisWeek = await _userService.GetAllUsersWithBirthdayForWeekAsync(today, false);

        if (unannouncedBirthdaysThisWeek.IsFailed)
        {
            _logger.LogError("Failed getting un-announced birthdays");
            return;
        }

        if (!unannouncedBirthdaysThisWeek.Value.Any())
        {
            _logger.LogInformation("Found no un-announced birthdays this week");
            return;
        }

        // TODO This could be changed to query birthday job documents whose UserId matches any of the users who have birthdays this week

        var birthdayJobs = await _birthdayJobService.GetAllAsync();
        if (birthdayJobs.IsFailed)
        {
            _logger.LogError("Failed getting birthday jobs");
            return;
        }

        var unscheduledBirthdays = unannouncedBirthdaysThisWeek.Value.Where(user => !birthdayJobs.Value.Contains(new BirthdayJobDocument { UserId = user.Id })).ToList();
        if (!unscheduledBirthdays.Any())
        {
            _logger.LogDebug("All birthdays for this week are already scheduled");
        }

        foreach (var unscheduledBirthday in unscheduledBirthdays)
        {
            var result = await _birthdayJobService.UpsertAsync(new BirthdayJobDocument
            {
                Id = unscheduledBirthday.Id,
                UserId = unscheduledBirthday.Id,
                BirthdayDate = unscheduledBirthday.GetBirthdateInEst(today.Year)!.Value
            });

           if (result.IsFailed)
           {
               _logger.LogWarning("Birthday job creation failed for an unscheduled birthday {Id}", unscheduledBirthday.Id);
           }
        }
    }
}
