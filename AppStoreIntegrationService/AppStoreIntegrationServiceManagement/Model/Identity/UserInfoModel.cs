using System.ComponentModel.DataAnnotations;

namespace AppStoreIntegrationServiceManagement.Model.Identity
{
    public class UserInfoModel
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Role { get; set; }

        public bool IsCurrentUser { get; set; }

        public bool IsBuiltInAdmin { get; set; }
    }
}
