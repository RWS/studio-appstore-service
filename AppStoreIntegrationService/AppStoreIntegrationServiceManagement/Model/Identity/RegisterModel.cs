using System.ComponentModel.DataAnnotations;

namespace AppStoreIntegrationServiceManagement.Model.Identity
{
    public class RegisterModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "Invalid e-mail address!")]
        public string Email { get; set; }

        [Required]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[^\s]{8,}$", ErrorMessage = "The password is to weak!")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "User role")]
        public string UserRole { get; set; }
        public bool IsNewAccount { get; set; }
        public string SalesForceId { get; set; }
        public string OosId { get; set; }
        public string SalesForceName { get; set; }

        public bool IsEmpty()
        {
            return new[] { Password, ConfirmPassword, Username }.All(x => x == null);
        }

    }
}
