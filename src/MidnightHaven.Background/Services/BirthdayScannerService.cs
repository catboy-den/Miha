using Cronos;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MidnightHaven.Logic.Services.Interfaces;

namespace MidnightHaven.Background.Services;

/// <summary>
/// Scans birthdays, and creates BirthdayJobDocuments
/// </summary>
public class BirthdayScannerService : BackgroundService
{
    private const string Schedule = "0,30 ? * * *"; // https://crontab.cronhub.io/

    private readonly IUserService _userService;
    private readonly ILogger<BirthdayScannerService> _logger;

    private readonly CronExpression _cron;

    public BirthdayScannerService(
        IUserService userService,
        ILogger<BirthdayScannerService> logger)
    {
        _userService = userService;
        _logger = logger;

        _cron = CronExpression.Parse(Schedule, CronFormat.Standard);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            //await CheckScheduledEventsAsync();

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
        // GetAllWithBirthdaysThisWeek, will get all user documents whose birthdays are this week (est)
        // in this service, remove users whose LastBirthdateAnnouncement (year) is not this year], and remove users who have a birthdayJobId too
        // Create new job documents, if the job doc is created, set the users birthdayJobId to the new job doc id
    }
}
