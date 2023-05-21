using NodaTime;

namespace MidnightHaven.Shared.ZonedClocks.Interfaces;

public interface IZonedClock : IClock
{
    /// <summary>
    /// Returns the current instant provided by the underlying clock, adjusted
    /// to the time zone of this object.
    /// </summary>
    /// <returns>The current instant provided by the underlying clock, adjusted to the
    /// time zone of this object.</returns>
    public ZonedDateTime GetCurrentZonedDateTime();

    /// <summary>
    /// Returns the local date/time of the current instant provided by the underlying clock, adjusted
    /// to the time zone of this object.
    /// </summary>
    /// <returns>The local date/time of the current instant provided by the underlying clock, adjusted to the
    /// time zone of this object.</returns>
    public LocalDateTime GetCurrentLocalDateTime();

    /// <summary>
    /// Returns the offset date/time of the current instant provided by the underlying clock, adjusted
    /// to the time zone of this object.
    /// </summary>
    /// <returns>The offset date/time of the current instant provided by the underlying clock, adjusted to the
    /// time zone of this object.</returns>
    public OffsetDateTime GetCurrentOffsetDateTime();

    /// <summary>
    /// Returns the local date of the current instant provided by the underlying clock, adjusted
    /// to the time zone of this object.
    /// </summary>
    /// <returns>The local date of the current instant provided by the underlying clock, adjusted to the
    /// time zone of this object.</returns>
    public LocalDate GetCurrentDate();

    /// <summary>
    /// Returns the local time of the current instant provided by the underlying clock, adjusted
    /// to the time zone of this object.
    /// </summary>
    /// <returns>The local time of the current instant provided by the underlying clock, adjusted to the
    /// time zone of this object.</returns>
    public LocalTime GetCurrentTimeOfDay();
}
