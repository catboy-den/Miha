using Miha.Shared.ZonedClocks.Interfaces;
using NodaTime;
using NodaTime.TimeZones;

namespace Miha.Shared.ZonedClocks;

public class EasternStandardZonedClock(IClock clock) : ZonedClock(clock, DateTimeZoneProviders.Tzdb[Timezones.IanaEasternTime], CalendarSystem.Iso), IEasternStandardZonedClock
{
    private readonly DateTimeZone _timeZone = DateTimeZoneProviders.Tzdb[Timezones.IanaEasternTime];
    private readonly BclDateTimeZone _bclDateTimeZone = (BclDateTimeZone) DateTimeZoneProviders.Bcl[Timezones.IanaEasternTime];
    private readonly TimeZoneInfo _timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(Timezones.WindowsEasternTime);

    public DateTimeZone GetTzdbTimeZone() => _timeZone;
    public BclDateTimeZone GetBclTimeZone() => _bclDateTimeZone;
    public TimeZoneInfo GetTimeZoneInfo() => _timeZoneInfo;
}
