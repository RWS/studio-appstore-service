using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;

namespace AppStoreIntegrationServiceCore.Repository
{
    public class NamesRepository : INamesRepository
    {
        private readonly IResponseManager _responseManager;

        public NamesRepository(IResponseManager responseManager)
        {
            _responseManager = responseManager;
        }

        public async Task<IEnumerable<NameMapping>> GetAllNames()
        {
            var data = await _responseManager.GetResponse();
            return data.Names;
        }

        public async Task<IEnumerable<NameMapping>> GetAllNames(List<string> pluginsNames)
        {
            pluginsNames ??= new List<string>();
            var nameMappings = await GetAllNames();
            return pluginsNames.SelectMany(pluginName => nameMappings.Where(n => n.OldName == pluginName));
        }

        public async Task<bool> TryUpdateMapping(NameMapping mapping)
        {
            var mappings = (await GetAllNames()).ToList();
            if (mapping == null || mappings.Any(x => x.IsDuplicate(mapping)))
            {
                return false;
            }

            var index = mappings.IndexOf(mappings.FirstOrDefault(c => c.Id == mapping.Id));
            if (index >= 0)
            {
                mappings[index] = mapping;
            }
            else
            {
                mappings.Add(mapping);
            }

            await SaveNames(mappings);
            return true;
            
        }

        public async Task DeleteMapping(string id)
        {
            var names = await GetAllNames();
            await SaveNames(names.Where(item => item.Id != id));
        }

        private async Task SaveNames(IEnumerable<NameMapping> names)
        {
            var data = await _responseManager.GetResponse();
            data.Names = names;
            await _responseManager.SaveResponse(data);
        }
    }
}
