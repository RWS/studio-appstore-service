namespace AppStoreIntegrationService.Model
{
    public class VersionStructureModel
    {
        public VersionStructureModel(PluginVersion pluginVersion, int versionPartialCounter)
        {
            PluginVersion = pluginVersion;
            VersionPartialCounter = versionPartialCounter;
        }

        public PluginVersion PluginVersion { get; set; }

        public int VersionPartialCounter { get; set; }
    }
}
