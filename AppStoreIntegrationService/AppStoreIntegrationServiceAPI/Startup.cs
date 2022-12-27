using System.IO.Compression;
using Microsoft.AspNetCore.ResponseCompression;
using static AppStoreIntegrationServiceCore.Enums;
using AppStoreIntegrationServiceCore.Repository;
using AppStoreIntegrationServiceCore.Repository.Interface;
using AppStoreIntegrationServiceAPI.Model.Repository.Interface;
using AppStoreIntegrationServiceAPI.Model.Repository;

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

            if (deployMode == DeployMode.AzureBlob)
            {
                services.AddSingleton<IResponseManager, AzureRepository>();
                services.AddSingleton<IPluginManager, AzureRepository>();
                services.AddSingleton<IProductsManager, AzureRepository>();
                services.AddSingleton<IVersionManager, AzureRepository>();
                services.AddSingleton<INamesManager, AzureRepository>();
                services.AddSingleton<ICategoriesManager, AzureRepository>();
                services.AddSingleton<ISettingsManager, AzureRepository>();
            }
            else
            {
                services.AddSingleton<IResponseManager, LocalRepository>();
                services.AddSingleton<IPluginManager, LocalRepository>();
                services.AddSingleton<IProductsManager, LocalRepository>();
                services.AddSingleton<IVersionManager, LocalRepository>();
                services.AddSingleton<INamesManager, LocalRepository>();
                services.AddSingleton<ICategoriesManager, LocalRepository>();
                services.AddSingleton<ISettingsManager, LocalRepository>();
            }

            services.AddSingleton<IConfigurationSettings>(configurationSettings);
            services.AddSingleton<ICategoriesRepository, CategoriesRepository>();
            services.AddSingleton<IPluginRepository, PluginRepository>();
            services.AddSingleton<IProductsRepository, ProductsRepository>();
            services.AddSingleton<IPluginResponseConverter, PluginResponseConverter>();
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
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}"
                );
            });
        }
    }
}