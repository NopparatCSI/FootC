namespace WebApp1.Security;

public static class AdminRoles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string Admin = "Admin";
    public const string Packer = "Packer";

    public static readonly string[] All =
    [
        SuperAdmin,
        Admin,
        Packer
    ];

    // Function: IsValid
    // Purpose: Executes the IsValid workflow for this module.
    public static bool IsValid(string? role)
    {
        return !string.IsNullOrWhiteSpace(role)
            && All.Contains(role, StringComparer.OrdinalIgnoreCase);
    }

    // Function: Normalize
    // Purpose: Executes the Normalize workflow for this module.
    public static string Normalize(string? role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return Admin;
        }

        var normalizedRole = role.Trim();

        // Backward compatibility for old role names that may still exist in DB.
        if (string.Equals(normalizedRole, "CatalogManager", StringComparison.OrdinalIgnoreCase)
            || string.Equals(normalizedRole, "Support", StringComparison.OrdinalIgnoreCase))
        {
            return Admin;
        }

        if (string.Equals(normalizedRole, "OrderManager", StringComparison.OrdinalIgnoreCase))
        {
            return Packer;
        }

        return All.FirstOrDefault(item => string.Equals(item, normalizedRole, StringComparison.OrdinalIgnoreCase))
            ?? Admin;
    }
}
