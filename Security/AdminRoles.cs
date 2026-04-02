namespace WebApp1.Security;

public static class AdminRoles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string CatalogManager = "CatalogManager";
    public const string OrderManager = "OrderManager";
    public const string Support = "Support";

    public static readonly string[] All =
    [
        SuperAdmin,
        CatalogManager,
        OrderManager,
        Support
    ];

    public static bool IsValid(string? role)
    {
        return !string.IsNullOrWhiteSpace(role)
            && All.Contains(role, StringComparer.OrdinalIgnoreCase);
    }

    public static string Normalize(string? role)
    {
        return All.FirstOrDefault(item => string.Equals(item, role, StringComparison.OrdinalIgnoreCase))
            ?? Support;
    }
}