using AppStoreIntegrationServiceManagement.Areas.Identity;
using AppStoreIntegrationServiceManagement.Areas.Identity.Data;
using AppStoreIntegrationServiceManagement.Model.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

[assembly: HostingStartup(typeof(IdentityHostingStartup))]
namespace AppStoreIntegrationServiceManagement.Areas.Identity
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
                    })
                    .AddEntityFrameworkStores<AppStoreIntegrationServiceContext>();

                services.ConfigureApplicationCookie(options =>
                {
                    options.LoginPath = "/Identity/Authentication/Login";
                });
                services.AddTransient<IUserSeed, UserSeed>();
            });
        }
    }
}
