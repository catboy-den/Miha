using MidnightHaven.Shared.ZonedClocks.Interfaces;
using NodaTime;

namespace MidnightHaven.Shared.ZonedClocks;

public class EasternStandardZonedClock : ZonedClock, IEasternStandardZonedClock
{
    public EasternStandardZonedClock(IClock clock) : base(clock, DateTimeZoneProviders.Tzdb[Timezones.EasternStandardTime], CalendarSystem.Iso)
    {

    }
}
