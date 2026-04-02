using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp1.ViewModels;

public class AdminListViewModel
{
    public List<AdminListItemViewModel> Admins { get; set; } = [];

    public AdminCreateViewModel CreateAdmin { get; set; } = new();

    public List<SelectListItem> RoleOptions { get; set; } = [];

    public string CurrentAdminRole { get; set; } = string.Empty;
}