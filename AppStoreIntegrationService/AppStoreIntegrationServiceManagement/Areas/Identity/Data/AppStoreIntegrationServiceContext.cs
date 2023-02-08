using AppStoreIntegrationServiceManagement.Model.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AppStoreIntegrationServiceManagement.Areas.Identity.Data;

public class AppStoreIntegrationServiceContext : IdentityDbContext<IdentityUserExtended>
{
    public AppStoreIntegrationServiceContext(DbContextOptions<AppStoreIntegrationServiceContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);

        builder.Entity<IdentityUserExtended>().Property(e => e.EmailNotificationsEnabled);
        builder.Entity<IdentityUserExtended>().Property(e => e.PushNotificationsEnabled);
    }
}
