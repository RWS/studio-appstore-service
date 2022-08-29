using Microsoft.AspNetCore.Mvc;

namespace AppStoreIntegrationServiceManagement.Model
{
    public class ProfileModel
    {
        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }
    }
}
