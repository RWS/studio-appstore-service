using AppStoreIntegrationServiceCore.DataBase.Interface;
using AppStoreIntegrationServiceCore.DataBase.Models;
using Microsoft.EntityFrameworkCore;

namespace AppStoreIntegrationServiceCore.DataBase
{
    public class AppStoreIntegrationServiceContext : DbContext
    {
        public AppStoreIntegrationServiceContext(DbContextOptions<AppStoreIntegrationServiceContext> options) : base(options) { }

        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<UserAccount> UserAccounts { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<AccountEntitlement> AccountEntitlements { get; set; }
        public DbSet<AccountAgreement> AccountAgreements { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
