using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceManagement.Model
{
    public class PluginsRenameModel
    {
        public IEnumerable<NameMapping> NamesMapping { get; set; }

        public NameMapping NewNameMapping { get; set; }
    }
}
