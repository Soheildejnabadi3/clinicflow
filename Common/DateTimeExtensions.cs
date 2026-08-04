namespace ClinicFlow.Api.Common;

public static class DateTimeExtensions
{
    /// <summary>
    /// Normalises a DateTime coming off a JSON request body to UTC.
    /// System.Text.Json yields Kind=Unspecified for "2026-08-05T14:00:00" and
    /// Kind=Local for values carrying an offset. Comparing either against
    /// DateTime.UtcNow, or returning it in a response, would be off by the local
    /// offset, so pin the value to UTC before it is used or stored.
    /// </summary>
    public static DateTime ToUtc(this DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };

    public static DateTime? ToUtc(this DateTime? value) =>
        value.HasValue ? value.Value.ToUtc() : null;
}
