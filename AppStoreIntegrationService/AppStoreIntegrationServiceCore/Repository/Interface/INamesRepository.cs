using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceCore.Repository.Interface
{
    public interface INamesRepository
    {
        /// <summary>
        /// Get new name for a list of plugins
        /// </summary>
        /// <param name="pluginsNames">List of the name of the plugins for which we want to get the new name if exists</param>
        /// <returns>List of name mappings if exist</returns>
        Task<IEnumerable<NameMapping>> GetAllNameMappings(List<string> pluginsNames);

        Task<IEnumerable<NameMapping>> GetAllNameMappings();
        /// <summary>
        /// Reads all the name mappings from local file path
        /// </summary>
        Task<List<NameMapping>> ReadLocalNameMappings(string nameMappingsFilePath);

        Task UpdateNamesMapping(List<NameMapping> namesMapping);

        Task DeleteNameMapping(string id);
    }
}
