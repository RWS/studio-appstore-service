using AppStoreIntegrationServiceCore.Repository.Interface;
using Microsoft.AspNetCore.Hosting;
using static AppStoreIntegrationServiceCore.Enums;

namespace AppStoreIntegrationServiceCore.Repository
{
    public class ConfigurationSettings : IConfigurationSettings
    {
        public string StorageAccountName { get; set; }
        public string StorageAccountKey { get; set; }
        public string BlobName { get; set; }
        public string LocalFolderPath { get; set; }
        public string PluginsFileName { get; set; }
        public string InstrumentationKey { get; set; }
        public DeployMode DeployMode { get; set; }
        public string ConfigFolderPath { get; set; }
        public string LocalPluginsFilePath { get; set; }
        public string PluginsFileBackUpPath { get; set; }
        public string SettingsFileName { get; set; }
        public string NotificationsFileName { get; set; }
        public string NotificationsFilePath { get; set; }
        public string SendGridAPIKey { get; set; }

        public ConfigurationSettings(DeployMode deployMode)
        {
            DeployMode = deployMode;
        }

        public async Task SetFilePathsProperties(IWebHostEnvironment environment)
        {
            switch (DeployMode)
            {
                case DeployMode.AzureBlob:
                    return;
                case DeployMode.ServerFilePath:
                    ConfigFolderPath = $"{Path.GetFullPath(Path.Combine(environment.ContentRootPath, "..\\"))}{LocalFolderPath}";
                    break;
                case DeployMode.NetworkFilePath:
                    ConfigFolderPath = LocalFolderPath;
                    break;
            }

            if (!string.IsNullOrEmpty(PluginsFileName))
            {
                LocalPluginsFilePath = Path.Combine(ConfigFolderPath, PluginsFileName);
                PluginsFileBackUpPath = Path.Combine(ConfigFolderPath, $"{Path.GetFileNameWithoutExtension(PluginsFileName)}_backup.json");
            }

            if (!string.IsNullOrEmpty(NotificationsFileName))
            {
                NotificationsFilePath = Path.Combine(ConfigFolderPath, NotificationsFileName);
            }

            await CreateConfigurationFiles();
        }

        private async Task CreateConfigurationFiles()
        {
            if (string.IsNullOrEmpty(ConfigFolderPath))
            {
                return;
            }

            Directory.CreateDirectory(ConfigFolderPath);
            if (!string.IsNullOrEmpty(LocalPluginsFilePath) && !File.Exists(LocalPluginsFilePath))
            {
                await File.Create(LocalPluginsFilePath).DisposeAsync();
            }

            if (!string.IsNullOrEmpty(PluginsFileBackUpPath) && !File.Exists(PluginsFileBackUpPath))
            {
                await File.Create(PluginsFileBackUpPath).DisposeAsync();
            }

            if (!string.IsNullOrEmpty(NotificationsFilePath) && !File.Exists(NotificationsFilePath))
            {
                await File.Create(NotificationsFilePath).DisposeAsync();
            }
        }

        public void LoadVariables()
        {
            StorageAccountName = GetVariable(ServiceResource.StorageAccountName);
            BlobName = GetVariable(ServiceResource.BlobName);
            LocalFolderPath = GetVariable(ServiceResource.LocalFolderPath);
            PluginsFileName = GetVariable(ServiceResource.PluginsFileName);
            SettingsFileName = GetVariable(ServiceResource.SettingsFileName);
            SendGridAPIKey = GetVariable(ServiceResource.SendGridAPIKey);
            NotificationsFileName = GetVariable(ServiceResource.NotificationsFileName);
        }

        private static string GetVariable(string key)
        {
            return Environment.GetEnvironmentVariable(key) ?? Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.User);
        }
    }
}
