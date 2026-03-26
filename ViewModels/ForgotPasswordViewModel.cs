using System.ComponentModel.DataAnnotations;

namespace WebApp1.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
