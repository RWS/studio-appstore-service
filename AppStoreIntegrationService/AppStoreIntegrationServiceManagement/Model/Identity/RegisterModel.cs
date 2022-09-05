using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace AppStoreIntegrationServiceManagement.Model.Identity
{
    public class RegisterModel
    {
        [BindProperty]
        public RegisterInputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        [TempData]
        public string StatusMessage { get; set; }
    }
}
