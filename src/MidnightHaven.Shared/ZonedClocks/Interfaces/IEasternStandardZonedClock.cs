using NodaTime;
using NodaTime.TimeZones;

namespace MidnightHaven.Shared.ZonedClocks.Interfaces;

public interface IEasternStandardZonedClock : IZonedClock
{
    public DateTimeZone GetTzdbTimeZone();
    public BclDateTimeZone GetBclTimeZone();
    public TimeZoneInfo GetTimeZoneInfo();
}
