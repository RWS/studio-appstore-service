using AppStoreIntegrationServiceCore.DataBase.Models;

namespace AppStoreIntegrationServiceCore.DataBase.Interface
{
    public interface IUserRolesManager
    {
        public List<UserRole> Roles { get; }
        void AddRole(UserRole role);
        UserRole GetRoleById(string id);
        UserRole GetRoleByName(string name);
        string GetRoleId(string name);
    }
}
