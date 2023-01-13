using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceCore.Repository.Interface
{
    public interface ICommentsManager
    {
        public Task<IDictionary<int, CommentPackage>> ReadComments();
        public Task UpdateComments(IDictionary<int, CommentPackage> package);
    }
}
