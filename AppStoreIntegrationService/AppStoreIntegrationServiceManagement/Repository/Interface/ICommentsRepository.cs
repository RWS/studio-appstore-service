using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceManagement.Repository.Interface
{
    public interface ICommentsRepository
    {
        Task<IEnumerable<Comment>> GetComments(int pluginId, string versionId = null);
        Task DeleteComment(int commentId, int pluginId, string versionId = null);
        Task DeleteComments(int pluginId, string versionId = null);
        Task SaveComment(Comment comment, int pluginId, string versionId = null);
        Task<Comment> GetComment(int pluginId, int commentId, string versionId = null);
    }
}
