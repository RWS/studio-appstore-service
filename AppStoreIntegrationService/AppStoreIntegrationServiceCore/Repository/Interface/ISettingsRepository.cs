using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceCore.Repository.Interface
{
    public interface ISettingsRepository
    {
        Task<SiteSettings> GetSettings();

        Task SaveSettings(SiteSettings settings);
    }
}
