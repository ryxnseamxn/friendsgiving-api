namespace Friendsgiving.Api.Infrastructure;

internal static class Input
{
    public const int MaxLength = 100;

    /// <summary>Trims free text and caps its length; returns null when blank.</summary>
    public static string? Clean(string? value)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed)) return null;
        return trimmed.Length > MaxLength ? trimmed[..MaxLength] : trimmed;
    }

    /// <summary>Postgres <c>timestamptz</c> columns only accept UTC; treat zone-less input as UTC.</summary>
    public static DateTime ToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Unspecified => DateTime.SpecifyKind(value, DateTimeKind.Utc),
        _ => value.ToUniversalTime(),
    };
}
