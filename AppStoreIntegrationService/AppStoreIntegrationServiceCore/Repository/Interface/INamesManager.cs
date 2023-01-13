using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceCore.Repository.Interface
{
    public interface INamesManager
    {
        Task SaveNames(IEnumerable<NameMapping> names);
        Task<IEnumerable<NameMapping>> ReadNames();
    }
}
