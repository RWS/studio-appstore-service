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
        public string MappingFileName { get; set; }
        public string SettingsFileName { get; set; }
        public string InstrumentationKey { get; set; }
        public DeployMode DeployMode { get; set; }
        public string NameMappingsFilePath { get; set; }
        public string ConfigFolderPath { get; set; }
        public string LocalPluginsFilePath { get; set; }
        public string PluginsFileBackUpPath { get; set; }
        public string CommentsFilePath { get; set; }
        public string CommentsFileName { get; set; }
        public string LogsFilePath { get; set; }
        public string LogsFileName { get; set; }
        public Task SetFilePathsProperties(IWebHostEnvironment environment);
    }
}
