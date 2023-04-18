using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;

namespace AppStoreIntegrationServiceCore.Repository
{
    public class NamesRepositoryBase : INamesRepositoryReadonly
    {
        private readonly IResponseManager _responseManager;

        public NamesRepositoryBase(IResponseManager responseManager)
        {
            _responseManager = responseManager;
        }

        public async Task<IEnumerable<NameMapping>> GetAllNames()
        {
            var data = await _responseManager.GetBaseResponse();
            return data.Names;
        }

        public async Task<IEnumerable<NameMapping>> GetAllNames(List<string> pluginsNames)
        {
            pluginsNames ??= new List<string>();
            var nameMappings = await GetAllNames();
            return pluginsNames.SelectMany(pluginName => nameMappings.Where(n => n.OldName == pluginName));
        }
    }
}