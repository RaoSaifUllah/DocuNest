namespace DocuNest.Domain.Policies;

public static class ReminderCalendar
{
    /// <summary>DST gaps advance to the first valid local minute; overlaps choose the earlier UTC instant.</summary>
    public static DateTimeOffset Resolve(DateOnly date, TimeOnly time, string ianaZone)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ianaZone);
        var zone = TimeZoneInfo.FindSystemTimeZoneById(ianaZone);
        var local = DateTime.SpecifyKind(date.ToDateTime(time), DateTimeKind.Unspecified);
        for (var movedMinutes = 0; zone.IsInvalidTime(local); movedMinutes++)
        {
            if (movedMinutes >= 24 * 60) throw new ArgumentException("Timezone gap exceeds the supported one-day bound.", nameof(ianaZone));
            local = local.AddMinutes(1);
        }
        if (zone.IsAmbiguousTime(local))
            return new DateTimeOffset(local, zone.GetAmbiguousTimeOffsets(local).Max()).ToUniversalTime();
        return new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(local, zone), TimeSpan.Zero);
    }

    public static bool WithinDeliveryGrace(DateTimeOffset due, DateTimeOffset now) => due <= now && now - due <= TimeSpan.FromHours(24);
}
