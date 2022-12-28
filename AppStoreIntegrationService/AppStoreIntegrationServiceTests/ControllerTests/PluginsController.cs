﻿using Xunit;
using AppStoreIntegrationServiceManagement.Controllers.Plugins;
using NSubstitute;
using AppStoreIntegrationServiceCore.Repository.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;

namespace AppStoreIntegrationServiceTests
{
    public class PluginsControllerTest
    {
        [Fact]
        public async void PluginsController_OnIndexInvoke_ReturnsTheCorrespongingView()
        {
            var mockPluginRepository = Substitute.For<IPluginRepository>();
            var mockProductsRepository = Substitute.For<IProductsRepository>();
            var mockContextAccesor = Substitute.For<IHttpContextAccessor>();
            var mockCategoriesRepository = Substitute.For<ICategoriesRepository>();
            var mockTempDataProvider = Substitute.For<ITempDataProvider>();
            var mockCommentsRepository = Substitute.For<ICommentsRepository>();
            var mockWebHostEnvironment = Substitute.For<IWebHostEnvironment>();
            var pluginsController = new PluginsController(mockPluginRepository, mockContextAccesor, mockProductsRepository, mockCategoriesRepository)
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
