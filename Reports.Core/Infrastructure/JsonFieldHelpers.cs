using System.Text.Json;

namespace Reports.Core.Infrastructure;

internal static class JsonFieldHelpers
{
    public static string GetStringField(this JsonElement fields, string name, string fallback = "")
    => fields.TryGetProperty(name, out var el) && el.ValueKind == JsonValueKind.String ? el.GetString()! : fallback;

    public static DateTime GetDateField(this JsonElement fields, string name)
    => fields.TryGetProperty(name, out var el) && el.ValueKind == JsonValueKind.String ? DateTime.Parse(el.GetString()!)
    : fields.TryGetProperty(name, out el) && el.ValueKind == JsonValueKind.Number ? el.GetDateTime() : DateTime.MinValue;

    public static string? GetIdentityDisplayName(this JsonElement fields)
    {
        if (!fields.TryGetProperty("System.AssignedTo", out var el)) return null;
        // Identity can be string (legacy) or object with displayName
        return el.ValueKind switch
        {
            JsonValueKind.Object => el.TryGetProperty("displayName", out var dn) ? dn.GetString() : null,
            JsonValueKind.String => el.GetString(),
            _ => null
        };
    }

    public static string? GetIdentityUniqueName(this JsonElement fields)
    {
        if (!fields.TryGetProperty("System.AssignedTo", out var el)) return null;
        return el.ValueKind == JsonValueKind.Object && el.TryGetProperty("uniqueName", out var un)
        ? un.GetString()
        : null;
    }
}