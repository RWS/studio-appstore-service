﻿using AppStoreIntegrationServiceCore.Repository.Interface;
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
        public string MappingFileName { get; set; }
        public string InstrumentationKey { get; set; }
        public DeployMode DeployMode { get; set; }
        public string NameMappingsFilePath { get; set; }
        public string CommentsFilePath { get; set; }
        public string ConfigFolderPath { get; set; }
        public string LocalPluginsFilePath { get; set; }
        public string PluginsFileBackUpPath { get; set; }
        public string SettingsFileName { get; set; }
        public string CommentsFileName { get; set; }

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

            if (!string.IsNullOrEmpty(MappingFileName))
            {
                NameMappingsFilePath = Path.Combine(ConfigFolderPath, MappingFileName);
            }

            if (!string.IsNullOrEmpty(CommentsFileName))
            {
                CommentsFilePath = Path.Combine(ConfigFolderPath, CommentsFileName);
            }

            if (!string.IsNullOrEmpty(PluginsFileName))
            {
                LocalPluginsFilePath = Path.Combine(ConfigFolderPath, PluginsFileName);
                PluginsFileBackUpPath = Path.Combine(ConfigFolderPath, $"{Path.GetFileNameWithoutExtension(PluginsFileName)}_backup.json");
            }

            await CreateConfigurationFiles();
        }

        private async Task CreateConfigurationFiles()
        {
            if (!string.IsNullOrEmpty(ConfigFolderPath))
            {
                Directory.CreateDirectory(ConfigFolderPath);
                if (!string.IsNullOrEmpty(LocalPluginsFilePath) && !File.Exists(LocalPluginsFilePath))
                {
                    await File.Create(LocalPluginsFilePath).DisposeAsync();
                }

                if (!string.IsNullOrEmpty(NameMappingsFilePath) && !File.Exists(NameMappingsFilePath))
                {
                    await File.Create(NameMappingsFilePath).DisposeAsync();
                }

                if (!string.IsNullOrEmpty(CommentsFilePath) && !File.Exists(CommentsFilePath))
                {
                    await File.Create(CommentsFilePath).DisposeAsync();
                }

                if (!string.IsNullOrEmpty(PluginsFileBackUpPath) && !File.Exists(PluginsFileBackUpPath))
                {
                    await File.Create(PluginsFileBackUpPath).DisposeAsync();
                }
            }
        }

        public void LoadVariables()
        {
            StorageAccountName = GetVariable(ServiceResource.StorageAccountName);
            StorageAccountKey = GetVariable(ServiceResource.StorageAccountKey);
            BlobName = GetVariable(ServiceResource.BlobName);
            LocalFolderPath = GetVariable(ServiceResource.LocalFolderPath);
            PluginsFileName = GetVariable(ServiceResource.PluginsFileName);
            InstrumentationKey = GetVariable(ServiceResource.TelemetryInstrumentationKey);
            MappingFileName = GetVariable(ServiceResource.MappingFileName);
            SettingsFileName = GetVariable(ServiceResource.SettingsFileName);
            CommentsFileName = GetVariable(ServiceResource.CommentsFileName);
        }

        private static string GetVariable(string key)
        {
            return Environment.GetEnvironmentVariable(key) ?? Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.User);
        }
    }
}
