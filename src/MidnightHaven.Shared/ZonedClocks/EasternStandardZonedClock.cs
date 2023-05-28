using MidnightHaven.Shared.ZonedClocks.Interfaces;
using NodaTime;
using NodaTime.TimeZones;

namespace MidnightHaven.Shared.ZonedClocks;

public class EasternStandardZonedClock : ZonedClock, IEasternStandardZonedClock
{
    private readonly DateTimeZone _timeZone;
    private readonly BclDateTimeZone _bclDateTimeZone;
    private readonly TimeZoneInfo _timeZoneInfo;

    public EasternStandardZonedClock(IClock clock) : base(clock, DateTimeZoneProviders.Tzdb[Timezones.EasternStandardTime], CalendarSystem.Iso)
    {
        _timeZone = DateTimeZoneProviders.Tzdb[Timezones.EasternStandardTime];
        _bclDateTimeZone = (BclDateTimeZone) DateTimeZoneProviders.Bcl[Timezones.EasternStandardTime];
        _timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(Timezones.EasternStandardTime);
    }

    public DateTimeZone GetTzdbTimeZone() => _timeZone;
    public BclDateTimeZone GetBclTimeZone() => _bclDateTimeZone;
    public TimeZoneInfo GetTimeZoneInfo() => _timeZoneInfo;
}
