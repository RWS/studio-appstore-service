using AppStoreIntegrationServiceCore.DataBase.Models;
using AppStoreIntegrationServiceManagement.DataBase.Interface;

namespace AppStoreIntegrationServiceManagement.Model.Identity
{
    public class ExtendedAccount : Account
    {
        private readonly UserProfile _signedInUserProfile;

        public ExtendedAccount() { }

        public ExtendedAccount
        (
            IUserAccountsManager userAccountsManager,
            UserProfile signedInUserProfile,
            Account account
        ) : base(account)
        {
            _signedInUserProfile = signedInUserProfile;
            Role = userAccountsManager.GetUserRoleForAccount(signedInUserProfile, this).Name;
            UserId = signedInUserProfile.Id;
            CanLeaveAccount = !IsDefaultAdmin() && userAccountsManager.CanBeRemovedFromAccount(signedInUserProfile, this);
        }

        public string Role { get; }

        public string UserId { get; }

        public bool CanLeaveAccount { get; }

        public string GetMessage()
        {
            if (_signedInUserProfile.IsBuiltInAdmin())
            {
                return $"The built-in Administrator cannot leave {Name}!";
            }

            if (CanLeaveAccount)
            {
                return null;
            }

            return $"You are the only Administrator of {Name}! To be able to leave, a new Administrator should be assigned to this Account!";
        }

        private bool IsDefaultAdmin()
        {
            return _signedInUserProfile.IsBuiltInAdmin() && Name == "AppStore Account";
        }
    }
}