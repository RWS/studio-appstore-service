using AppStoreIntegrationServiceAPI;
using AppStoreIntegrationServiceCore.Data;
using AppStoreIntegrationServiceCore.DataBase;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

[assembly: HostingStartup(typeof(IdentityHostingStartup))]
namespace AppStoreIntegrationServiceAPI
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
                services.AddDbContext<AppStoreIntegrationServiceContext>(options =>
                    options.UseSqlServer(
                        context.Configuration.GetConnectionString("AppStoreIntegrationServiceContextConnection")));

                services.AddIdentity<IdentityUserExtended, IdentityRole>(
                    options =>
                    {
                        options.Password.RequireDigit = false;
                        options.Password.RequireLowercase = false;
                        options.Password.RequireUppercase = false;
                        options.Password.RequireNonAlphanumeric = false;
                        options.SignIn.RequireConfirmedAccount = false;
                        options.User.RequireUniqueEmail = true;
                    })
                    .AddEntityFrameworkStores<AppStoreIntegrationServiceContext>();
            });
        }
    }
}
