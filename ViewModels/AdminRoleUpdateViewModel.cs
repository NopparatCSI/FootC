using System.ComponentModel.DataAnnotations;

namespace WebApp1.ViewModels;

public class AdminRoleUpdateViewModel
{
    [Required]
    public string AdminUserId { get; set; } = string.Empty;

    [Required]
    public string RoleName { get; set; } = string.Empty;
}