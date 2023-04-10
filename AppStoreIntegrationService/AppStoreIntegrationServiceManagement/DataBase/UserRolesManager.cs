using AppStoreIntegrationServiceCore.DataBase.Interface;
using AppStoreIntegrationServiceCore.DataBase.Models;
using AppStoreIntegrationServiceManagement.DataBase.Interface;

namespace AppStoreIntegrationServiceCore.DataBase
{
    public class UserRolesManager : IUserRolesManager
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

        public async Task<int> AddRole(UserRole role)
        {
            if (string.IsNullOrEmpty(role?.Id) || string.IsNullOrEmpty(role?.Name))
            {
                return 0;
            }

            using (var context = _serviceContext.CreateContext())
            {
                context.UserRoles.Add(role);
                return await context.SaveChangesAsync();
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
    }
}