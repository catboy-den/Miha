using Cronos;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MidnightHaven.Logic.Services.Interfaces;
using MidnightHaven.Redis.Documents;
using MidnightHaven.Shared.ZonedClocks.Interfaces;

namespace MidnightHaven.Background.Services;

/// <summary>
/// Scans birthdays, and creates BirthdayJobDocuments
/// </summary>
public class BirthdayScannerService : BackgroundService
{
    private const string Schedule = "0,30 ? * * *"; // https://crontab.cronhub.io/

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
            var utcNow = DateTime.UtcNow;
            var nextUtc = _cron.GetNextOccurrence(utcNow);

            if (nextUtc is null)
            {
                _logger.LogWarning("Next utc occurence is null");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                continue;
            }

            await ScanBirthdaysAsync();

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

        var birthdayJobs = await _birthdayJobService.GetAllAsync();

        if (birthdayJobs.IsFailed)
        {
            _logger.LogError("Failed getting birthday jobs");
            return;
        }

        var unscheduledBirthdays = unannouncedBirthdaysThisWeek.Value.Where(user => !birthdayJobs.Value.Contains(new BirthdayJobDocument { UserDocumentId = user.Id })).ToList();

        if (!unscheduledBirthdays.Any())
        {
            _logger.LogInformation("All birthdays for this week are already scheduled");
        }

        foreach (var unscheduledBirthday in unscheduledBirthdays)
        {
            var result = await _birthdayJobService.UpsertAsync(new BirthdayJobDocument
            {
                Id = unscheduledBirthday.Id,
                UserDocumentId = unscheduledBirthday.Id,
                BirthdayDate = unscheduledBirthday.GetBirthdateInEst(today.Year)!.Value
            });

           if (result.IsFailed)
           {
               _logger.LogWarning("Birthday job creation failed for an unscheduled birthday {Id}", unscheduledBirthday.Id);
           }
        }
    }
}
