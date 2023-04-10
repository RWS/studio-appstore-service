using AppStoreIntegrationServiceCore.DataBase.Models;

namespace AppStoreIntegrationServiceManagement.DataBase.Interface
{
    public interface IUserRolesManager
    {
        public List<UserRole> Roles { get; }
        Task<int>  AddRole(UserRole role);
        UserRole GetRoleById(string id);
        UserRole GetRoleByName(string name);
    }
}
