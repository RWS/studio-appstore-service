using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;

namespace AppStoreIntegrationServiceCore.Repository
{
    public class NamesRepository : INamesRepository
    {
        private readonly INamesManager _namesManager;

        public NamesRepository(INamesManager namesManager)
        {
            _namesManager = namesManager;
        }

        public async Task<IEnumerable<NameMapping>> GetAllNameMappings()
        {
            return await _namesManager.ReadNames();
        }

        public async Task<IEnumerable<NameMapping>> GetAllNameMappings(List<string> pluginsNames)
        {
            var nameMappings = await _namesManager.ReadNames();
            return pluginsNames.Select(pluginName => nameMappings
                               .FirstOrDefault(n => n.OldName.Equals(pluginName)))
                               .Where(mapping => mapping != null);
        }

        public async Task UpdateMappings(List<NameMapping> names)
        {
            await _namesManager.SaveNames(names);
        }

        public async Task DeleteMapping(string id)
        {
            var newNames = (await _namesManager.ReadNames()).Where(item => item.Id != id).ToList();
            await UpdateMappings(newNames);
        }
    }
}
