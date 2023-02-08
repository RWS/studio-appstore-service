namespace AppStoreIntegrationServiceManagement.Model.Identity
{
    public class UserInfoModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public bool IsCurrentUser { get; set; }
    }
}
