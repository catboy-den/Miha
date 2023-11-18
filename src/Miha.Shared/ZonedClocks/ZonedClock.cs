using Miha.Shared.ZonedClocks.Interfaces;
using NodaTime;
using NodaTime.Calendars;
using NodaTime.Extensions;

namespace Miha.Shared.ZonedClocks;

/// <summary>
/// A clock with an associated time zone and calendar. This is effectively a convenience
/// class decorating an <see cref="IClock"/>.
/// </summary>
/// <threadsafety>This type is immutable reference type. See the thread safety section of the user guide for more information.</threadsafety>
public class ZonedClock : IZonedClock
{
    /// <summary>Gets the clock used to provide the current instant.</summary>
    /// <value>The clock associated with this zoned clock.</value>
    public IClock Clock { get; }

    /// <summary>Gets the time zone used when converting the current instant into a zone-sensitive value.</summary>
    /// <value>The time zone associated with this zoned clock.</value>
    public DateTimeZone Zone { get; }

    /// <summary>Gets the calendar system used when converting the current instant into a calendar-sensitive value.</summary>
    /// <value>The calendar system associated with this zoned clock.</value>
    public CalendarSystem Calendar { get; }

    /// <summary>
    /// Creates a new <see cref="ZonedClock"/> with the given clock, time zone and calendar system.
    /// </summary>
    /// <param name="clock">Clock to use to obtain instants.</param>
    /// <param name="zone">Time zone to adjust instants into.</param>
    /// <param name="calendar">Calendar system to use.</param>
    public ZonedClock(IClock clock, DateTimeZone zone, CalendarSystem calendar)
    {
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(zone);
        ArgumentNullException.ThrowIfNull(calendar);

        Clock = clock;
        Zone = zone;
        Calendar = calendar;
    }

    /// <inheritdoc/>
    public Instant GetCurrentInstant() => Clock.GetCurrentInstant();

    /// <inheritdoc/>
    public ZonedDateTime GetCurrentZonedDateTime() => GetCurrentInstant().InZone(Zone, Calendar);

    /// <inheritdoc/>
    public LocalDateTime GetCurrentLocalDateTime() => GetCurrentZonedDateTime().LocalDateTime;

    /// <inheritdoc/>
    public OffsetDateTime GetCurrentOffsetDateTime() => GetCurrentZonedDateTime().ToOffsetDateTime();

    /// <inheritdoc/>
    public LocalDate GetCurrentDate() => GetCurrentZonedDateTime().Date;

    /// <inheritdoc/>
    public LocalTime GetCurrentTimeOfDay() => GetCurrentZonedDateTime().TimeOfDay;

    /// <inheritdoc/>
    public int GetCurrentWeek() => WeekYearRules.Iso.GetWeekOfWeekYear(GetCurrentDate());

    /// <inheritdoc/>
    public IEnumerable<LocalDate> GetCurrentWeekAsDates(IsoDayOfWeek isoDayOfWeek = IsoDayOfWeek.Monday)
    {
        // Get the current date
        var currentDate = GetCurrentDate();

        // Get the current week in year
        var currentWeek = WeekYearRules.Iso.GetWeekOfWeekYear(currentDate);

        // Get the first day of the week (Monday) for the current week
        var firstDayOfWeek = LocalDate.FromWeekYearWeekAndDay(currentDate.Year, currentWeek, isoDayOfWeek);
        
        var daysOfWeek = new List<LocalDate>();
        for (var i = 0; i < 7; i++)
        {
            var day = firstDayOfWeek.PlusDays(i);
            daysOfWeek.Add(day);
        }

        return daysOfWeek;
    }

    /// <inheritdoc/>
    public ZonedDateTime ToZonedDateTime(DateTimeOffset offset) => offset.ToZonedDateTime().WithZone(Zone);
}
