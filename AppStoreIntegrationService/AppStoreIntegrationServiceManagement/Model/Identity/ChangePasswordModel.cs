using Microsoft.AspNetCore.Mvc;

namespace AppStoreIntegrationServiceManagement.Model.Identity
{
    public class ChangePasswordModel
    {
        [BindProperty]
        public ChangePasswordInputModel Input { get; set; }
    }
}
