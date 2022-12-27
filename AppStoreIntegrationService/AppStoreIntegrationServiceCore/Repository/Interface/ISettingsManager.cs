using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceCore.Repository.Interface
{
    public interface ISettingsManager
    {
        Task<SiteSettings> ReadSettings();
        Task SaveSettings(SiteSettings settings);
    }
}
