using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;

namespace AppStoreIntegrationServiceCore.Repository
{
    public class CategoriesRepositoryBase : ICategoriesRepositoryReadonly
    {
        private readonly IResponseManager _responseManager;

        public CategoriesRepositoryBase(IResponseManager responseManager)
        {
            _responseManager = responseManager;
        }

        public async Task<IEnumerable<CategoryDetails>> GetAllCategories()
        {
            var data = await _responseManager.GetBaseResponse();
            return data.Categories;
        }
    }
}