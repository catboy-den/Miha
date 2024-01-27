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
public class BirthdayScannerService(
    IEasternStandardZonedClock easternStandardZonedClock,
    IUserService userService,
    IBirthdayJobService birthdayJobService,
    ILogger<BirthdayScannerService> logger) : BackgroundService
{
    private const string Schedule = "*/5 8-19 * * *"; // https://crontab.cronhub.io/

    private readonly CronExpression _cron = CronExpression.Parse(Schedule, CronFormat.Standard);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await ScanBirthdaysAsync();

            var utcNow = easternStandardZonedClock.GetCurrentInstant().ToDateTimeUtc();
            var nextUtc = _cron.GetNextOccurrence(DateTimeOffset.UtcNow, easternStandardZonedClock.GetTimeZoneInfo());

            if (nextUtc is null)
            {
                logger.LogWarning("Next utc occurence is null");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                continue;
            }

            logger.LogDebug("Waiting {Time}", nextUtc.Value - utcNow);
            await Task.Delay(nextUtc.Value - utcNow, stoppingToken);
        }
    }

    private async Task ScanBirthdaysAsync()
    {
        logger.LogInformation("Scanning for birthdays");

        var today = easternStandardZonedClock.GetCurrentDate();
        var unannouncedBirthdaysThisWeek = await userService.GetAllUsersWithBirthdayForWeekAsync(today, false);

        if (unannouncedBirthdaysThisWeek.IsFailed)
        {
            logger.LogError("Failed getting un-announced birthdays");
            return;
        }

        if (!unannouncedBirthdaysThisWeek.Value.Any())
        {
            logger.LogInformation("Found no un-announced birthdays this week");
            return;
        }

        var birthdayJobs = await birthdayJobService.GetAllAsync();

        if (birthdayJobs.IsFailed)
        {
            logger.LogError("Failed getting birthday jobs");
            return;
        }

        var unscheduledBirthdays = unannouncedBirthdaysThisWeek.Value.Where(user => !birthdayJobs.Value.Contains(new BirthdayJobDocument { UserDocumentId = user.Id })).ToList();

        if (!unscheduledBirthdays.Any())
        {
            logger.LogInformation("All birthdays for this week are already scheduled");
        }

        foreach (var unscheduledBirthday in unscheduledBirthdays)
        {
            var result = await birthdayJobService.UpsertAsync(new BirthdayJobDocument
            {
                Id = unscheduledBirthday.Id,
                UserDocumentId = unscheduledBirthday.Id,
                BirthdayDate = unscheduledBirthday.GetBirthdateInEst(today.Year)!.Value
            });

           if (result.IsFailed)
           {
               logger.LogWarning("Birthday job creation failed for an unscheduled birthday {Id}", unscheduledBirthday.Id);
           }
        }
    }
}
