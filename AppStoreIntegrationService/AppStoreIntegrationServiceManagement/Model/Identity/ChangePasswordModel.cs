using Microsoft.AspNetCore.Mvc;

namespace AppStoreIntegrationServiceManagement.Model.Identity
{
    public class ChangePasswordModel
    {
        [BindProperty]
        public ChangePasswordInputModel Input { get; set; }

        public bool IsCurrentUserSelected { get; set; }

        public string Id { get; set; }

        public string Username { get; set; }
    }
}
