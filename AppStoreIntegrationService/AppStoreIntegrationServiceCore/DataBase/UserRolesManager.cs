using AppStoreIntegrationServiceCore.DataBase.Interface;
using AppStoreIntegrationServiceCore.DataBase.Models;

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
                using var context = _serviceContext.CreateContext();
                return context.UserRoles.ToList();
            }
        }

        public void AddRole(UserRole role)
        {
            using var context = _serviceContext.CreateContext();
            context.UserRoles.Add(role);
            context.SaveChanges();
        }

        public UserRole GetRoleById(string id)
        {
            using var context = _serviceContext.CreateContext();
            return context.UserRoles.ToList().FirstOrDefault(x => x.Id == id);
        }

        public UserRole GetRoleByName(string name)
        {
            using var context = _serviceContext.CreateContext();
            return context.UserRoles.ToList().FirstOrDefault(x => x.Name == name);
        }

        public string GetRoleId(string name)
        {
            using var context = _serviceContext.CreateContext();
            return context.UserRoles.ToList().FirstOrDefault(x => x.Name == name)?.Id;
        }
    }
}
