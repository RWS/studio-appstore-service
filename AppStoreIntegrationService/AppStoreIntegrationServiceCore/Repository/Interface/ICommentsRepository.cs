using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceCore.Repository.Interface
{
    public interface ICommentsRepository
    {
        Task<IEnumerable<Comment>> GetCommentsForVersion(string pluginName, string versionId);
        Task DeleteVersionComment(int id, string pluginName, string versionId);
        Task SaveComment(Comment comment, string pluginName, string versionId);
        Task<Comment> GetVersionComment(string pluginName, int id, string versionId);
    }
}
