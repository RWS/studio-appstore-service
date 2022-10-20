using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceCore.Repository.V2.Interface
{
    public interface ISettingsRepository
    {
        Task<SiteSettings> GetSettings();

        Task SaveSettings(SiteSettings settings);
    }
}
