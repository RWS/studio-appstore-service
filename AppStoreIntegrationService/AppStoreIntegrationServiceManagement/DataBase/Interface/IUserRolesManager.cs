using AppStoreIntegrationServiceCore.DataBase.Models;
using Microsoft.AspNetCore.Identity;

namespace AppStoreIntegrationServiceManagement.DataBase.Interface
{
    public interface IUserRolesManager
    {
        List<UserRole> Roles { get; }
        bool IsAdmin(string roleId);
        Task<IdentityResult> TryAddRole(UserRole role);
        UserRole GetRoleById(string id);
        UserRole GetRoleByName(string name);
    }
}
