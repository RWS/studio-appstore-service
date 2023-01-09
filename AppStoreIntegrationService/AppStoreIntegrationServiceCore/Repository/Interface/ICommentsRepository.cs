using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceCore.Repository.Interface
{
    public interface ICommentsRepository
    {
        Task<IEnumerable<Comment>> GetComments(string pluginName, string versionId = null);
        Task DeleteComment(int id, string pluginName, string versionId = null);
        Task DeleteComments(string pluginName, string versionId = null);
        Task SaveComment(Comment comment, string pluginName, string versionId = null);
        Task<Comment> GetComment(string pluginName, int id, string versionId = null);
    }
}
