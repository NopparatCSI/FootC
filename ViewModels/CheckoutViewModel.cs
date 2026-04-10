using System.ComponentModel.DataAnnotations;

namespace WebApp1.ViewModels;

public class CheckoutViewModel
{
    [Required]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Phone")]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Address Line")]
    public string AddressLine { get; set; } = string.Empty;

    [Required]
    [Display(Name = "District")]
    public string District { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Province")]
    public string Province { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Postal Code")]
    public string PostalCode { get; set; } = string.Empty;

    [Display(Name = "Order Notes")]
    public string? Notes { get; set; }

    [Required]
    public string PaymentMethod { get; set; } = "COD";

    [Display(Name = "Promo Code")]
    [StringLength(50)]
    public string? PromoCode { get; set; }

    [Required]
    public string CartItemsJson { get; set; } = "[]";
}
