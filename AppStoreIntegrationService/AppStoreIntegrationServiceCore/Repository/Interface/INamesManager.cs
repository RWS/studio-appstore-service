using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceCore.Repository.Interface
{
    public interface INamesManager
    {
        Task SaveNames(List<NameMapping> names);
        Task<List<NameMapping>> ReadNames();
    }
}
