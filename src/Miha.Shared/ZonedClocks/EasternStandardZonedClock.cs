using Miha.Shared.ZonedClocks.Interfaces;
using NodaTime;
using NodaTime.TimeZones;

namespace Miha.Shared.ZonedClocks;

public class EasternStandardZonedClock : ZonedClock, IEasternStandardZonedClock
{
    private readonly DateTimeZone _timeZone;
    private readonly BclDateTimeZone _bclDateTimeZone;
    private readonly TimeZoneInfo _timeZoneInfo;

    public EasternStandardZonedClock(IClock clock) : base(clock, DateTimeZoneProviders.Tzdb[Timezones.IanaEasternTime], CalendarSystem.Iso)
    {
        _timeZone = DateTimeZoneProviders.Tzdb[Timezones.IanaEasternTime];
        _bclDateTimeZone = (BclDateTimeZone) DateTimeZoneProviders.Bcl[Timezones.IanaEasternTime];
        _timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(Timezones.WindowsEasternTime);
    }

    public DateTimeZone GetTzdbTimeZone() => _timeZone;
    public BclDateTimeZone GetBclTimeZone() => _bclDateTimeZone;
    public TimeZoneInfo GetTimeZoneInfo() => _timeZoneInfo;
}
