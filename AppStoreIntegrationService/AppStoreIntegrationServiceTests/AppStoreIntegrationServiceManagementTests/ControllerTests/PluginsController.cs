using Xunit;
using AppStoreIntegrationServiceManagement.Controllers.Plugins;
using NSubstitute;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using AppStoreIntegrationServiceManagement.Repository.Interface;
using AppStoreIntegrationServiceManagement.DataBase.Interface;
using AppStoreIntegrationServiceCore.DataBase.Interface;

namespace AppStoreIntegrationServiceTests.AppStoreIntegrationServiceManagementTests.ControllerTests
{
    public class PluginsControllerTest
    {
        [Fact]
        public async void PluginsController_OnIndexInvoke_ReturnsTheCorrespongingView()
        {
            var mockUserProfilesManager = Substitute.For<IUserProfilesManager>();
            var mockUserAccouuntsManager = Substitute.For<IUserAccountsManager>();
            var mockPluginRepository = Substitute.For<IPluginRepository>();
            var mockProductsRepository = Substitute.For<IProductsRepository>();
            var mockContextAccesor = Substitute.For<IHttpContextAccessor>();
            var mockCategoriesRepository = Substitute.For<ICategoriesRepository>();
            var mockTempDataProvider = Substitute.For<ITempDataProvider>();
            var mockCommentsRepository = Substitute.For<ICommentsRepository>();
            var mockLogginRepository = Substitute.For<ILoggingRepository>();
            var mockNotificationCenter = Substitute.For<INotificationCenter>();
            var mockAccountsManager = Substitute.For<IAccountsManager>();

            var pluginsController = new PluginsController
            (
                mockUserProfilesManager,
                mockUserAccouuntsManager,
                mockPluginRepository,
                mockProductsRepository,
                mockCategoriesRepository,
                mockCommentsRepository,
                mockLogginRepository,
                mockNotificationCenter,
                mockAccountsManager
            )

            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = mockContextAccesor.HttpContext
                },
                TempData = new TempDataDictionary(mockContextAccesor.HttpContext, mockTempDataProvider)
            };

            Assert.Equal("ConfigToolModel", ((ViewResult)await pluginsController.Index()).Model.GetType().Name);
            await mockProductsRepository.ReceivedWithAnyArgs(1).GetAllProducts();
            await mockPluginRepository.ReceivedWithAnyArgs(1).GetAll(default);
        }
    }
}
