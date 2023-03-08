﻿using Microsoft.AspNetCore.Identity;

namespace AppStoreIntegrationServiceManagement.Model.DataBase
{
    public class IdentityUserExtended : IdentityUser
    {
        public bool EmailNotificationsEnabled { get; set; }
        public bool PushNotificationsEnabled { get; set; }
        public bool IsBuiltInAdmin { get; set; }
        public string SelectedAccountId { get; set; }
    }
}
