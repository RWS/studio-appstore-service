using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository;
using AppStoreIntegrationServiceCore.Repository.Interface;
using AppStoreIntegrationServiceManagement.Repository.Interface;

namespace AppStoreIntegrationServiceManagement.Repository
{
    public class NamesRepository : NamesRepositoryBase, INamesRepository
    {
        private readonly IResponseManager _responseManager;

        public NamesRepository(IResponseManager responseManager) : base(responseManager)
        {
            _responseManager = responseManager;
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
