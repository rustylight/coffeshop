using CoffeeShop.Models;

namespace CoffeeShop.Helpers;

public static class SessionManager
{
    public static User? CurrentUser { get; set; }
    public static Shift? CurrentShift { get; set; }

    public static string GetInitials()
    {
        if (CurrentUser == null) return "??";
        var parts = CurrentUser.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2)
            return $"{parts[0][0]}{parts[1][0]}";
        return parts.Length > 0 ? $"{parts[0][0]}" : "??";
    }

    public static string GetShortName()
    {
        if (CurrentUser == null) return "";
        var parts = CurrentUser.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 3)
            return $"{parts[0]} {parts[1][0]}.{parts[2][0]}.";
        if (parts.Length == 2)
            return $"{parts[0]} {parts[1][0]}.";
        return parts.Length > 0 ? parts[0] : "";
    }

    public static string RoleName => CurrentUser?.Role?.Name ?? "";

    public static void Clear()
    {
        CurrentUser = null;
        CurrentShift = null;
    }
}
