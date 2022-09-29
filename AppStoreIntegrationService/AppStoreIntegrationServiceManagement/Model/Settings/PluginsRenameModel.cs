using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceManagement.Model.Settings
{
    public class PluginsRenameModel
    {
        public IEnumerable<NameMapping> NamesMapping { get; set; }

        public NameMapping NewNameMapping { get; set; }
    }
}
