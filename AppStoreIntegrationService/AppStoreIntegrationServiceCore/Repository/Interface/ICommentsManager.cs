using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceCore.Repository.Interface
{
    public interface ICommentsManager
    {
        public Task<IDictionary<string, CommentPackage>> ReadComments();
        public Task UpdateComments(IDictionary<string, CommentPackage> package);
    }
}
