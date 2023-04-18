namespace AppStoreIntegrationServiceManagement.Model.Identity
{
    public class UsersModel
    {
        public IEnumerable<ExtendedUserProfile> ExtendedUsers { get; set; }
        public int UnconfirmedUsers { get; set; }
    }
}
