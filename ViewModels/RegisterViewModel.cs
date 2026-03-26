namespace WebApp1.ViewModels
{
    public class RegisterViewModel
    {
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string PhoneNumber { get; set; }
        public  DateOnly DateOfBirth { get; set; }
    }
}