using AppStoreIntegrationServiceCore.DataBase.Interface;
using Microsoft.Extensions.DependencyInjection;

namespace AppStoreIntegrationServiceCore.DataBase
{
    public class ServiceContextFactory : IServiceContextFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ServiceContextFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public AppStoreIntegrationServiceContext CreateContext()
        {
            return _serviceProvider.CreateScope().ServiceProvider.GetService<AppStoreIntegrationServiceContext>();
        }
    }
}
