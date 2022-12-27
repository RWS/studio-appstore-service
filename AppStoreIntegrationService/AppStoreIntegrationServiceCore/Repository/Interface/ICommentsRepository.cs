using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceCore.Repository.Interface
{
    public interface ICommentsRepository
    {
        Task<IEnumerable<Comment>> GetCommentsForPlugin(string pluginName);
        Task DeleteComment(int id, string pluginName);
        Task SaveComment(Comment comment, string pluginName);
        Task<Comment> GetPluginComment(string pluginName, int id);
    }
}
