using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceCore.Repository.Common.Interface
{
    public interface ISettingsRepository
    {
        Task<SiteSettings> GetSettings();

        Task SaveSettings(SiteSettings settings);
    }
}
