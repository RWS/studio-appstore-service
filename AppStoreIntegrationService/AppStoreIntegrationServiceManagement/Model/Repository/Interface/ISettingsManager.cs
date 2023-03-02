using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceManagement.Model.Repository.Interface
{
    public interface ISettingsManager
    {
        Task<SiteSettings> ReadSettings();
        Task SaveSettings(SiteSettings settings);
    }
}
