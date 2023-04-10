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
        public string UserRole { get; set; }
        public string SalesForceId { get; set; }
        public string OosId { get; set; }
        public string SalesForceName { get; set; }

        public bool IsEmpty()
        {
            return new[] { Password, Username }.All(x => x == null);
        }

    }
}
