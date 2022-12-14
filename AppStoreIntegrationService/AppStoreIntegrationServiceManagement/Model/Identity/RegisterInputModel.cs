using System.ComponentModel.DataAnnotations;

namespace AppStoreIntegrationServiceManagement.Model.Identity
{
    public class RegisterInputModel
    {
        [Required]
        [Display(Name = "Username")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "Invalid e-mail address!")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "User role")]
        public string UserRole { get; set; }

        public bool IsEmpty()
        {
            return new[] { UserName, Password, ConfirmPassword, Email }.All(x => x == null);
        }
    }
}
