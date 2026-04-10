using System.ComponentModel.DataAnnotations;
using WebApp1.Security;

namespace WebApp1.ViewModels;

public class AdminEditViewModel
{
    [Required]
    public string AdminUserId { get; set; } = string.Empty;

    [Required]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Birth Date")]
    public DateOnly? BirthDate { get; set; }

    [Required]
    [Display(Name = "Role")]
    public string RoleName { get; set; } = AdminRoles.Admin;

    [DataType(DataType.Password)]
    [StringLength(80, MinimumLength = 6)]
    [Display(Name = "New Password")]
    public string? Password { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Confirm New Password")]
    [Compare(nameof(Password), ErrorMessage = "Password confirmation does not match.")]
    public string? ConfirmPassword { get; set; }
}