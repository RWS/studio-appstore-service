using System.ComponentModel.DataAnnotations;

namespace AppStoreIntegrationServiceManagement.Model.Identity
{
    public class LoginInputModel
    {
        [Required]
        [EmailAddress(ErrorMessage = "E-mail address is in wrong format!")]
        [Display(Name = "Email")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
