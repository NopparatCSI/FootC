namespace WebApp1.ViewModels;

public class AdminListItemViewModel
{
    public string AdminUserId { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string BirthDateText { get; set; } = "-";

    public string RoleName { get; set; } = string.Empty;

    public string RoleBadgeClass { get; set; } = string.Empty;
}