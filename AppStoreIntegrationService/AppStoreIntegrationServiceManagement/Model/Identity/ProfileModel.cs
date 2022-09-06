using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace AppStoreIntegrationServiceManagement.Model.Identity
{
    public class ProfileModel
    {
        public string Username { get; set; }

        [Display(Name = "User role")]
        public string UserRole { get; set; }

        public bool IsUsernameEnabled { get; set; }
    }
}
