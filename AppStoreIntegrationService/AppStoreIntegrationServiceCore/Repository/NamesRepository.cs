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

        public async Task<bool> TryUpdateMapping(NameMapping mapping)
        {
            var mappings = await _namesManager.ReadNames();
            if (Exists(mappings, mapping))
            {
                return false;
            }

            var index = mappings.IndexOf(mappings.FirstOrDefault(c => c.Id.Equals(mapping.Id)));
            if (index >= 0)
            {
                mappings[index] = mapping;
            }
            else
            {
                mappings.Add(mapping);
            }

            await _namesManager.SaveNames(mappings);
            return true;
            
        }

        public async Task DeleteMapping(string id)
        {
            var newNames = (await _namesManager.ReadNames()).Where(item => item.Id != id).ToList();
            await _namesManager.SaveNames(newNames);
        }

        private static bool Exists(List<NameMapping> names, NameMapping mapping)
        {
            return names.Any(n => n.NewName == mapping.NewName &&
                             n.OldName == mapping.OldName &&
                             n.Id != mapping.Id);
        }
    }
}
