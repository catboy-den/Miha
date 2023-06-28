using Miha.Shared.ZonedClocks.Interfaces;
using NodaTime;

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

    /// <summary>
    /// Returns the current instant provided by the underlying clock.
    /// </summary>
    /// <returns>The current instant provided by the underlying clock.</returns>
    public Instant GetCurrentInstant() => Clock.GetCurrentInstant();

    /// <summary>
    /// Returns the current instant provided by the underlying clock, adjusted
    /// to the time zone of this object.
    /// </summary>
    /// <returns>The current instant provided by the underlying clock, adjusted to the
    /// time zone of this object.</returns>
    public ZonedDateTime GetCurrentZonedDateTime() => GetCurrentInstant().InZone(Zone, Calendar);

    /// <summary>
    /// Returns the local date/time of the current instant provided by the underlying clock, adjusted
    /// to the time zone of this object.
    /// </summary>
    /// <returns>The local date/time of the current instant provided by the underlying clock, adjusted to the
    /// time zone of this object.</returns>
    public LocalDateTime GetCurrentLocalDateTime() => GetCurrentZonedDateTime().LocalDateTime;

    /// <summary>
    /// Returns the offset date/time of the current instant provided by the underlying clock, adjusted
    /// to the time zone of this object.
    /// </summary>
    /// <returns>The offset date/time of the current instant provided by the underlying clock, adjusted to the
    /// time zone of this object.</returns>
    public OffsetDateTime GetCurrentOffsetDateTime() => GetCurrentZonedDateTime().ToOffsetDateTime();

    /// <summary>
    /// Returns the local date of the current instant provided by the underlying clock, adjusted
    /// to the time zone of this object.
    /// </summary>
    /// <returns>The local date of the current instant provided by the underlying clock, adjusted to the
    /// time zone of this object.</returns>
    public LocalDate GetCurrentDate() => GetCurrentZonedDateTime().Date;

    /// <summary>
    /// Returns the local time of the current instant provided by the underlying clock, adjusted
    /// to the time zone of this object.
    /// </summary>
    /// <returns>The local time of the current instant provided by the underlying clock, adjusted to the
    /// time zone of this object.</returns>
    public LocalTime GetCurrentTimeOfDay() => GetCurrentZonedDateTime().TimeOfDay;
}
