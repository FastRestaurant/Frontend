using System.Text;
using System.Text.Json;
using Blazored.LocalStorage;
using El_buen_sabor.Components.Models;

namespace El_buen_sabor.Components.Service;

public sealed class AuthSessionService
{
    private readonly ILocalStorageService _localStorage;

    public AuthSessionService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public async Task<string?> GetTokenAsync()
    {
        return await _localStorage.GetItemAsync<string>("token");
    }

    public async Task<bool> HasTokenAsync()
    {
        var token = await GetTokenAsync();
        return !string.IsNullOrWhiteSpace(token);
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var payload = await ReadPayloadAsync();
        if (payload is null || IsExpired(payload.Value))
        {
            await ClearSessionAsync();
            return false;
        }

        return true;
    }

    public async Task ClearSessionAsync()
    {
        await _localStorage.RemoveItemAsync("token");
        await _localStorage.RemoveItemAsync("refreshToken");
    }

    public async Task<Guid?> GetUserIdAsync()
    {
        var payload = await ReadPayloadAsync();
        if (payload is null)
            return null;

        foreach (var claim in payload.Value.EnumerateObject())
        {
            if (!IsUserIdClaim(claim.Name) || claim.Value.ValueKind != JsonValueKind.String)
                continue;

            if (Guid.TryParse(claim.Value.GetString(), out var userId))
                return userId;
        }

        return null;
    }

    public async Task<string?> GetRoleAsync()
    {
        var payload = await ReadPayloadAsync();
        if (payload is null)
            return null;

        foreach (var claim in payload.Value.EnumerateObject())
        {
            if (!IsRoleClaim(claim.Name))
                continue;

            var role = ReadClaimValue(claim.Value);
            if (!string.IsNullOrWhiteSpace(role))
                return NormalizeRole(role);
        }

        return null;
    }

    public async Task<bool> HasAnyRoleAsync(params string[] allowedRoles)
    {
        if (!await IsAuthenticatedAsync())
            return false;

        var role = await GetRoleAsync();
        if (string.IsNullOrWhiteSpace(role))
            return false;

        return allowedRoles.Any(allowed => NormalizeRole(allowed) == role);
    }

    public static string GetHomePathForRole(string? role)
    {
        return NormalizeRole(role) switch
        {
            AppRoles.Admin => "/DashboardSection",
            AppRoles.Waitress => "/waiter",
            AppRoles.Kitchen => "/kitchen",
            AppRoles.Cashier => "/DashboardSection",
            _ => "/DashboardSection"
        };
    }

    private async Task<JsonElement?> ReadPayloadAsync()
    {
        var token = await GetTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
            return null;

        var parts = token.Split('.');
        if (parts.Length < 2)
            return null;

        try
        {
            var payloadJson = Encoding.UTF8.GetString(Convert.FromBase64String(PadBase64(parts[1].Replace('-', '+').Replace('_', '/'))));
            using var document = JsonDocument.Parse(payloadJson);
            return document.RootElement.Clone();
        }
        catch
        {
            return null;
        }
    }

    private static string? ReadClaimValue(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.String)
            return value.GetString();

        if (value.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in value.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.String)
                    return item.GetString();
            }
        }

        return null;
    }

    private static bool IsExpired(JsonElement payload)
    {
        if (!payload.TryGetProperty("exp", out var expClaim))
            return true;

        long exp;
        if (expClaim.ValueKind == JsonValueKind.Number)
        {
            if (!expClaim.TryGetInt64(out exp))
                return true;
        }
        else if (expClaim.ValueKind == JsonValueKind.String)
        {
            if (!long.TryParse(expClaim.GetString(), out exp))
                return true;
        }
        else
        {
            return true;
        }

        var expiresAt = DateTimeOffset.FromUnixTimeSeconds(exp);
        return expiresAt <= DateTimeOffset.UtcNow;
    }

    private static string NormalizeRole(string? role)
    {
        var normalized = role?.Trim().ToUpperInvariant() ?? string.Empty;
        return normalized switch
        {
            AppRoles.AdminDisplay => AppRoles.Admin,
            AppRoles.KitchenDisplay => AppRoles.Kitchen,
            AppRoles.CashierDisplay => AppRoles.Cashier,
            AppRoles.WaitressDisplay => AppRoles.Waitress,
            "WAITER" => AppRoles.Waitress,
            "CAMARERA" => AppRoles.Waitress,
            "COCINA" => AppRoles.Kitchen,
            _ => normalized
        };
    }

    private static bool IsUserIdClaim(string claimName)
    {
        var normalized = claimName.Trim().ToLowerInvariant();
        return normalized is "sub" or "nameid" or "nameidentifier"
            || normalized.EndsWith("/nameidentifier", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsRoleClaim(string claimName)
    {
        var normalized = claimName.Trim().ToLowerInvariant();
        return normalized is "role" or "roles"
            || normalized.EndsWith("/role", StringComparison.OrdinalIgnoreCase);
    }

    private static string PadBase64(string value)
    {
        return value.PadRight(value.Length + (4 - value.Length % 4) % 4, '=');
    }
}
