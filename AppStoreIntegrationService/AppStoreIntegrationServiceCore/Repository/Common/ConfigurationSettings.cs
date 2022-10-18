using AppStoreIntegrationServiceCore.Repository.Common.Interface;
using Microsoft.AspNetCore.Hosting;

namespace AppStoreIntegrationServiceCore.Repository.Common
{
    public class ConfigurationSettings : IConfigurationSettings
    {
        public string StorageAccountName { get; set; }
        public string StorageAccountKey { get; set; }
        public string BlobName { get; set; }
        public string LocalFolderPath { get; set; }
        public string PluginsFileNameV1 { get; set; }
        public string PluginsFileNameV2 { get; set; }
        public string MappingFileName { get; set; }
        public string ProductsFileName { get; set; }
        public string InstrumentationKey { get; set; }
        public Enums.DeployMode DeployMode { get; set; }
        public string NameMappingsFilePath { get; set; }
        public string ProductsFilePath { get; set; }
        public string ConfigFolderPath { get; set; }
        public string LocalPluginsFilePathV1 { get; set; }
        public string LocalPluginsFilePathV2 { get; set; }
        public string PluginsFileBackUpPathV1 { get; set; }
        public string PluginsFileBackUpPathV2 { get; set; }
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

            if (!string.IsNullOrEmpty(PluginsFileNameV1))
            {
                LocalPluginsFilePathV1 = Path.Combine(ConfigFolderPath, PluginsFileNameV1);
                PluginsFileBackUpPathV1 = Path.Combine(ConfigFolderPath, $"{Path.GetFileNameWithoutExtension(PluginsFileNameV1)}_backup.json");
            }

            if (!string.IsNullOrEmpty(PluginsFileNameV2))
            {
                LocalPluginsFilePathV2 = Path.Combine(ConfigFolderPath, PluginsFileNameV2);
                PluginsFileBackUpPathV2 = Path.Combine(ConfigFolderPath, $"{Path.GetFileNameWithoutExtension(PluginsFileNameV2)}_backup.json");
            }

            await CreateConfigurationFiles();
        }

        private async Task CreateConfigurationFiles()
        {
            if (!string.IsNullOrEmpty(ConfigFolderPath))
            {
                Directory.CreateDirectory(ConfigFolderPath);
                if (!string.IsNullOrEmpty(LocalPluginsFilePathV1) && !File.Exists(LocalPluginsFilePathV1))
                {
                    await File.Create(LocalPluginsFilePathV1).DisposeAsync();
                }

                if (!string.IsNullOrEmpty(LocalPluginsFilePathV2) && !File.Exists(LocalPluginsFilePathV2))
                {
                    await File.Create(LocalPluginsFilePathV2).DisposeAsync();
                }

                if (!string.IsNullOrEmpty(NameMappingsFilePath) && !File.Exists(NameMappingsFilePath))
                {
                    await File.Create(NameMappingsFilePath).DisposeAsync();
                }

                if (!string.IsNullOrEmpty(ProductsFilePath) && !File.Exists(ProductsFilePath))
                {
                    await File.Create(ProductsFilePath).DisposeAsync();
                }

                if (!string.IsNullOrEmpty(PluginsFileBackUpPathV1) && !File.Exists(PluginsFileBackUpPathV1))
                {
                    await File.Create(PluginsFileBackUpPathV1).DisposeAsync();
                }

                if (!string.IsNullOrEmpty(PluginsFileBackUpPathV2) && !File.Exists(PluginsFileBackUpPathV2))
                {
                    await File.Create(PluginsFileBackUpPathV2).DisposeAsync();
                }
            }
        }

        public void LoadVariables()
        {
            StorageAccountName = GetVariable(ServiceResource.StorageAccountName);
            StorageAccountKey = GetVariable(ServiceResource.StorageAccountKey);
            BlobName = GetVariable(ServiceResource.BlobName);
            LocalFolderPath = GetVariable(ServiceResource.LocalFolderPath);
            PluginsFileNameV1 = GetVariable(ServiceResource.PluginsFileNameV1);
            PluginsFileNameV2 = GetVariable(ServiceResource.PluginsFileNameV2);
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
