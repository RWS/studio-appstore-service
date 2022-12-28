using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceCore.Repository.Interface
{
    public interface ICommentsManager
    {
        public Task<IDictionary<string, IDictionary<string, IEnumerable<Comment>>>> ReadComments();
        public Task UpdateComments(IDictionary<string, IDictionary<string, IEnumerable<Comment>>> comments);
    }
}
