using System.Net;
using System.IO.Compression;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.ResponseCompression;
using static AppStoreIntegrationServiceCore.Enums;
using AppStoreIntegrationServiceCore.Repository.Common;
using AppStoreIntegrationServiceCore.Repository.Common.Interface;
using AppStoreIntegrationServiceCore.Repository.V1;
using AppStoreIntegrationServiceCore.Repository.V2;
using AppStoreIntegrationServiceCore.Repository.V1.Interface;
using AppStoreIntegrationServiceCore.Repository.V2.Interface;
using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            var env = serviceProvider.GetService<IWebHostEnvironment>();

            var settingsDeployMode = Configuration.GetValue<string>("DeployMode");
            _ = Enum.TryParse(settingsDeployMode, out DeployMode deployMode);
            var configurationSettings = GetConfigurationSettings(env, deployMode).Result;

            services.AddMvc();
            services.AddHttpContextAccessor();
            services.Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Optimal;
            });

            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<GzipCompressionProvider>();
            });

            services.AddSingleton<IProductsRepository, ProductsRepository>();
            services.AddSingleton<IConfigurationSettings>(configurationSettings);
            services.AddSingleton<IAzureRepository<PluginDetails<PluginVersion<ProductDetails>>>, AzureRepository<PluginDetails<PluginVersion<ProductDetails>>>>();
            services.AddSingleton<IAzureRepositoryExtended<PluginDetails<PluginVersion<string>>>, AzureRepositoryExtended<PluginDetails<PluginVersion<string>>>>();
            services.AddSingleton<IPluginRepository<PluginDetails<PluginVersion<ProductDetails>>>, PluginRepository<PluginDetails<PluginVersion<ProductDetails>>>>();
            services.AddSingleton<IPluginRepositoryExtended<PluginDetails<PluginVersion<string>>>, PluginRepositoryExtended<PluginDetails<PluginVersion<string>>>>();
            services.AddSingleton<INamesRepository, NamesRepository>();
        }

        private async Task<ConfigurationSettings> GetConfigurationSettings(IWebHostEnvironment env, DeployMode deployMode)
        {
            var configurationSettings = new ConfigurationSettings(deployMode);
            var section = Configuration.GetSection("ConfigurationSettings");
            if (section.Exists())
            {
                Configuration.Bind("ConfigurationSettings", configurationSettings);
            }
            else
            {
                configurationSettings.LoadVariables();
            }

            await configurationSettings.SetFilePathsProperties(env);
            return configurationSettings;
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseResponseCaching();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}