using NodaTime;

namespace Miha.Shared.ZonedClocks.Interfaces;

public interface IZonedClock : IClock
{
    /// <summary>
    /// Returns the current instant provided by the underlying clock, adjusted
    /// to the time zone of this object.
    /// </summary>
    /// <returns>The current instant provided by the underlying clock, adjusted to the
    /// time zone of this <see cref="IZonedClock"/>.</returns>
    public ZonedDateTime GetCurrentZonedDateTime();

    /// <summary>
    /// Returns the local date/time of the current instant provided by the underlying clock, adjusted
    /// to the time zone of this <see cref="IZonedClock"/>.
    /// </summary>
    /// <returns>The local date/time of the current instant provided by the underlying clock, adjusted to the
    /// time zone of this <see cref="IZonedClock"/>.</returns>
    public LocalDateTime GetCurrentLocalDateTime();

    /// <summary>
    /// Returns the offset date/time of the current instant provided by the underlying clock, adjusted
    /// to the time zone of this <see cref="IZonedClock"/>.
    /// </summary>
    /// <returns>The offset date/time of the current instant provided by the underlying clock, adjusted to the
    /// time zone of this <see cref="IZonedClock"/>.</returns>
    public OffsetDateTime GetCurrentOffsetDateTime();

    /// <summary>
    /// Returns the local date of the current instant provided by the underlying clock, adjusted
    /// to the time zone of this <see cref="IZonedClock"/>.
    /// </summary>
    /// <returns>The local date of the current instant provided by the underlying clock, adjusted to the
    /// time zone of this <see cref="IZonedClock"/>.</returns>
    public LocalDate GetCurrentDate();

    /// <summary>
    /// Returns the local time of the current instant provided by the underlying clock, adjusted
    /// to the time zone of this <see cref="IZonedClock"/>.
    /// </summary>
    /// <returns>The local time of the current instant provided by the underlying clock, adjusted to the
    /// time zone of this <see cref="IZonedClock"/>.</returns>
    public LocalTime GetCurrentTimeOfDay();

    /// <summary>
    /// Returns the current week of the year, adjusted to the time zone of this <see cref="IZonedClock"/>, and abides
    /// by the ISO-8601 standard.
    /// </summary>
    /// <returns>The current week of the year, adjusted to the time zone of this <see cref="IZonedClock"/>.</returns>
    public int GetCurrentWeek();
    
    /// <summary>
    /// Returns an enumerable containing all the days of the current week, adjusted
    /// to the time zone of this <see cref="IZonedClock"/> and abides by the ISO-8601 standard.
    /// </summary>
    /// <returns>A list of dates of the current week, adjusted to the time zone of this <see cref="IZonedClock"/>.</returns>
    public IEnumerable<DateOnly> GetCurrentWeekAsDates(IsoDayOfWeek isoDayOfWeek = IsoDayOfWeek.Monday);

    /// <summary>
    /// Converts an offset to a <see cref="ZonedDateTime"/> using the time zone of this <see cref="IZonedClock"/>.
    /// </summary>
    /// <param name="offset"></param>
    /// <returns></returns>
    public ZonedDateTime ToZonedDateTime(DateTimeOffset offset);
}
