using AppStoreIntegrationServiceCore.DataBase.Models;
using AppStoreIntegrationServiceManagement.DataBase.Interface;

namespace AppStoreIntegrationServiceManagement.Model.Identity
{
    public class ExtendedUserProfile : UserProfile
    {
        private readonly IUserAccountsManager _userAccountsManager;
        private readonly UserProfile _signedInUserProfile;
        private readonly Account _selectedAccount;

        public ExtendedUserProfile() { }
        public ExtendedUserProfile
        (
            IUserAccountsManager userAccountsManager,
            UserProfile signedInUserProfile,
            Account selectedAccount,
            UserProfile userProfile
        ) : base(userProfile)
        {
            _userAccountsManager = userAccountsManager;
            _signedInUserProfile = signedInUserProfile;
            _selectedAccount = selectedAccount;
            Role = userAccountsManager.GetUserRoleForAccount(this, selectedAccount).Name;
            IsCurrentUser = Id == signedInUserProfile.Id;
            CanBeManaged = !IsDefaultAdmin() && CanBeRemovedFromAccount() && (IsSignedInUserAdmin() || IsCurrentUser);
        }

        public ExtendedUserProfile(UserProfile profile) : base(profile) { }

        public string Role { get; set; }
        public bool IsCurrentUser { get; }
        public bool CanBeManaged { get; }

        public string GetMessage()
        {
            if (IsBuiltInAdmin())
            {
                return "The built-in Administrator cannot be managed!";
            }

            if (CanBeRemovedFromAccount())
            {
                if (IsSignedInUserAdmin())
                {
                    return null;
                }

                return "You don't have the rights to remove this user!";
            }

            return "You are the only Administrator of this Account! To be able to leave, you have to assign a new Administrator for this Account!";
        }

        private bool IsDefaultAdmin() => IsBuiltInAdmin() && _selectedAccount.Name == "AppStore Account";

        private bool CanBeRemovedFromAccount() => _userAccountsManager.CanBeRemovedFromAccount(this, _selectedAccount);

        private bool IsSignedInUserAdmin()
        {
            var role = _userAccountsManager.GetUserRoleForAccount(_signedInUserProfile, _selectedAccount);
            return role.Name != "Developer";
        }
    }
}