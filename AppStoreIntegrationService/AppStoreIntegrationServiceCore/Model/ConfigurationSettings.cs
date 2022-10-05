using AppStoreIntegrationServiceCore.Repository.Interface;
using Microsoft.AspNetCore.Hosting;

namespace AppStoreIntegrationServiceCore.Model
{
    public class ConfigurationSettings : IConfigurationSettings
    {
        public string StorageAccountName { get; set; }
        public string StorageAccountKey { get; set; }
        public string BlobName { get; set; }
        public string LocalFolderPath { get; set; }
        public string ConfigFileName { get; set; }
        public string OosUri { get; set; }
        public string MappingFileName { get; set; }
        public string ProductsFileName { get; set; }
        public string InstrumentationKey { get; set; }
        public Enums.DeployMode DeployMode { get; set; }
        public string NameMappingsFilePath { get; set; }
        public string ProductsFilePath { get; set; }
        public string ConfigFolderPath { get; set; }
        public string LocalPluginsConfigFilePath { get; set; }
        public string ConfigFileBackUpPath { get; set; }
        public string SettingsFileName { get; set; }

        public ConfigurationSettings(Enums.DeployMode deployMode)
        {
            DeployMode = deployMode;
        }

        public async Task SetFilePathsProperties(IWebHostEnvironment environment)
        {
            switch (DeployMode)
            {
                case Enums.DeployMode.AzureBlob:
                    return;
                case Enums.DeployMode.ServerFilePath:
                    ConfigFolderPath = $"{environment.ContentRootPath}{LocalFolderPath}";
                    break;
                case Enums.DeployMode.NetworkFilePath:
                    ConfigFolderPath = LocalFolderPath;
                    break;
            }

            if (!string.IsNullOrEmpty(MappingFileName))
            {
                NameMappingsFilePath = Path.Combine(ConfigFolderPath, MappingFileName);
            }

            if (!string.IsNullOrEmpty(ProductsFileName))
            {
                ProductsFilePath = Path.Combine(ConfigFolderPath, ProductsFileName);
            }

            if (!string.IsNullOrEmpty(ConfigFileName))
            {
                LocalPluginsConfigFilePath = Path.Combine(ConfigFolderPath, ConfigFileName);
                ConfigFileBackUpPath = Path.Combine(ConfigFolderPath, $"{Path.GetFileNameWithoutExtension(ConfigFileName)}_backup.json");
            }

            await CreateConfigurationFiles();
        }

        private async Task CreateConfigurationFiles()
        {
            if (!string.IsNullOrEmpty(ConfigFolderPath))
            {
                Directory.CreateDirectory(ConfigFolderPath);
                if (!string.IsNullOrEmpty(LocalPluginsConfigFilePath) && !File.Exists(LocalPluginsConfigFilePath))
                {
                    await File.Create(LocalPluginsConfigFilePath).DisposeAsync();
                }

                if (!string.IsNullOrEmpty(NameMappingsFilePath) && !File.Exists(NameMappingsFilePath))
                {
                    await File.Create(NameMappingsFilePath).DisposeAsync();
                }

                if (!string.IsNullOrEmpty(ProductsFilePath) && !File.Exists(ProductsFilePath))
                {
                    await File.Create(ProductsFilePath).DisposeAsync();
                }

                if (!string.IsNullOrEmpty(ConfigFileBackUpPath) && !File.Exists(ConfigFileBackUpPath))
                {
                    await File.Create(ConfigFileBackUpPath).DisposeAsync();
                }
            }
        }

        public void LoadVariables()
        {
            StorageAccountName = GetVariable(ServiceResource.StorageAccountName);
            StorageAccountKey = GetVariable(ServiceResource.StorageAccountKey);
            BlobName = GetVariable(ServiceResource.BlobName);
            LocalFolderPath = GetVariable(ServiceResource.LocalFolderPath);
            ConfigFileName = GetVariable(ServiceResource.ConfigFileName);
            OosUri = GetVariable(ServiceResource.OosUri);
            InstrumentationKey = GetVariable(ServiceResource.TelemetryInstrumentationKey);
            MappingFileName = GetVariable(ServiceResource.MappingFileName);
            ProductsFileName = GetVariable(ServiceResource.ProductsFileName);
            SettingsFileName = GetVariable(ServiceResource.SettingsFileName);
        }

        private static string GetVariable(string key)
        {
            // by default it gets a process variable. Allow getting user as well
            return
                Environment.GetEnvironmentVariable(key) ??
                Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.User);
        }
    }
}
