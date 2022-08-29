using Microsoft.AspNetCore.Mvc;

namespace AppStoreIntegrationServiceManagement.Model
{
    public class ChangePasswordModel
    {
        [BindProperty]
        public ChangePasswordInputModel Input { get; set; }

        [TempData]
        public string StatusMessage { get; set; }
    }
}
