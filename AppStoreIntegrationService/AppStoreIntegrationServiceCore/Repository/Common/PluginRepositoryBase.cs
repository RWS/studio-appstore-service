using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Common.Interface;
using AppStoreIntegrationServiceCore.Repository.V1.Interface;

namespace AppStoreIntegrationServiceCore.Repository.Common
{
    public class PluginRepositoryBase<T>
    {
        private const int CategoryId_AutomatedTranslation = 6;
        private const int CategoryId_TranslationMemory = 3;
        private const int CategoryId_Terminology = 4;
        private const int CategoryId_FileFiltersConverters = 2;
        private const int CategoryId_Reference = 7;
        private const int CategoryId_ProcessReferenceAndAutomation = 5;

        protected readonly IConfigurationSettings _configurationSettings;
        protected readonly IAzureRepository<T> _azureRepository;
        private List<CategoryDetails> _availableCategories;

        public PluginRepositoryBase(IAzureRepository<T> azureRepository, IConfigurationSettings configurationSettings)
        {
            _azureRepository = azureRepository;
            _configurationSettings = configurationSettings;
            InitializeCategoryList();
        }

        public List<CategoryDetails> GetCategories()
        {
            return _availableCategories;
        }

        private void InitializeCategoryList()
        {
            _availableCategories = new List<CategoryDetails>
            {
                new CategoryDetails
                {
                    Name = ServiceResource.CategoryAutomatedTranslation,
                    Id = CategoryId_AutomatedTranslation
                },
                new CategoryDetails
                {
                    Name = ServiceResource.CategoryTranslationMemory,
                    Id = CategoryId_TranslationMemory
                },
                new CategoryDetails
                {
                    Name = ServiceResource.CategoryProcessAutomationAndManagement,
                    Id = CategoryId_ProcessReferenceAndAutomation
                },
                new CategoryDetails
                {
                    Name = ServiceResource.CategoryReference,
                    Id = CategoryId_Reference
                },
                new CategoryDetails
                {
                    Name = ServiceResource.CategoryTerminology,
                    Id = CategoryId_Terminology
                },
                new CategoryDetails
                {
                    Name = ServiceResource.CategoryFileFiltersConverters,
                    Id = CategoryId_FileFiltersConverters
                }
            };
        }
    }
}
