using AppStoreIntegrationServiceManagement.Model.Settings;

namespace AppStoreIntegrationServiceManagement.Repository.Interface
{
    public interface ISettingsManager
    {
        Task<SiteSettings> ReadSettings();
        Task SaveSettings(SiteSettings settings);
    }
}
