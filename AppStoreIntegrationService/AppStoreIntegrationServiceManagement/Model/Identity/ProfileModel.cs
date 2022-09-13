﻿using System.ComponentModel.DataAnnotations;

namespace AppStoreIntegrationServiceManagement.Model.Identity
{
    public class ProfileModel
    {
        [Required]
        public string Username { get; set; }

        [Display(Name = "User role")]
        public string UserRole { get; set; }

        public string Id { get; set; }

        public bool IsUsernameEditable { get; set; }

        public bool IsUserRoleEditable { get; set; }
    }
}