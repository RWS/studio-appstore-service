using Microsoft.AspNetCore.Hosting;
using static AppStoreIntegrationServiceCore.Enums;

namespace AppStoreIntegrationServiceCore.Repository.Interface
{
    public interface IConfigurationSettings
    {
        public string StorageAccountName { get; set; }
        public string StorageAccountKey { get; set; }
        public string BlobName { get; set; }
        public string LocalFolderPath { get; set; }
        public string PluginsFileName { get; set; }
        public string SettingsFileName { get; set; }
        public string NotificationsFileName { get; set; }
        public string InstrumentationKey { get; set; }
        public DeployMode DeployMode { get; set; }
        public string ConfigFolderPath { get; set; }
        public string LocalPluginsFilePath { get; set; }
        public string PluginsFileBackUpPath { get; set; }
        public string NotificationsFilePath { get; set; }
        public string SendGridAPIKey { get; set; }
        public Task SetFilePathsProperties(IWebHostEnvironment environment);
    }
}
