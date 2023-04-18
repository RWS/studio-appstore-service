using AppStoreIntegrationServiceCore.DataBase.Interface;
using AppStoreIntegrationServiceCore.DataBase.Models;
using AppStoreIntegrationServiceManagement.DataBase;
using AppStoreIntegrationServiceManagement.DataBase.Interface;
using Microsoft.AspNetCore.Identity;

namespace AppStoreIntegrationServiceCore.DataBase
{
    public class UserRolesManager : Manager, IUserRolesManager
    {
        private readonly IServiceContextFactory _serviceContext;

        public UserRolesManager(IServiceContextFactory serviceContext)
        {
            _serviceContext = serviceContext;
        }

        public List<UserRole> Roles
        {
            get
            {
                using (var context = _serviceContext.CreateContext())
                {
                    return context.UserRoles.ToList();
                }
            }
        }

        public async Task<IdentityResult> TryAddRole(UserRole role)
        {
            if (ExistNullParams(out var result, role?.Id, role?.Name))
            {
                return result;
            }

            try
            {
                using (var context = _serviceContext.CreateContext())
                {
                    context.UserRoles.Add(role);
                    await context.SaveChangesAsync();
                }

                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = ex.Message });
            }
        }

        public UserRole GetRoleById(string id)
        {
            using (var context = _serviceContext.CreateContext())
            {
                return context.UserRoles.ToList().FirstOrDefault(x => x.Id == id);
            }
        }

        public UserRole GetRoleByName(string name)
        {
            using (var context = _serviceContext.CreateContext())
            {
                return context.UserRoles.ToList().FirstOrDefault(x => x.Name == name);
            }
        }

        public bool IsAdmin(string roleId)
        {
            if (ExistNullParams(out _, roleId))
            {
                return false;
            }

            var role = GetRoleById(roleId);
            var roles = new[]
            {
                new UserRole { Name = "Administrator" },
                new UserRole { Name = "SystemAdministrator" },
            };

            return roles.Any(x => x.Equals(role));
        }
    }
}