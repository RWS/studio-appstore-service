using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace AppStoreIntegrationServiceManagement.Model.Identity
{
    public class RegisterModel
    {
        [Required]
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
        public string RoleId { get; set; }
        public SelectList Roles { get; set; }
        public IList<AuthenticationScheme> ExternalLogins { get; set; }
        public bool IsEmpty()
        {
            return new[] { Password, ConfirmPassword, UserName }.All(x => x == null);
        }

    }
}
