using System.IO.Compression;
using Microsoft.AspNetCore.ResponseCompression;
using AppStoreIntegrationServiceCore.Repository;
using AppStoreIntegrationServiceCore.Repository.Interface;
using AppStoreIntegrationServiceAPI.Model.Repository.Interface;
using AppStoreIntegrationServiceAPI.Model.Repository;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using AppStoreIntegrationServiceAPI.HealthChecks;
using HealthChecks.UI.Client;

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
            _ = Enum.TryParse(Configuration.GetValue<string>("DeployMode"), out DeployMode deployMode);
            var configurationSettings = GetConfigurationSettings(GetServiceProvider(services).GetService<IWebHostEnvironment>(), deployMode).Result;
            
            services.Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Optimal;
            });

            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<GzipCompressionProvider>();
            });

            services.AddMvc();
            services.AddHttpContextAccessor();
            services.AddHealthChecks().AddCheck<ResponeTimeHealthCheck>("Response time");

            if (deployMode == DeployMode.AzureBlob)
            {
                services.AddSingleton<IResponseManagerBase, AzureRepositoryBase>();
            }
            else
            {
                services.AddSingleton<IResponseManagerBase, LocalRepositoryBase>();
            }

            services.AddSingleton<IConfigurationSettings>(configurationSettings);
            services.AddSingleton<IPluginResponseConverter, PluginResponseConverter>();
        }

        private static ServiceProvider GetServiceProvider(IServiceCollection services)
        {
            return services.BuildServiceProvider();
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
                endpoints.MapHealthChecks("/health", new HealthCheckOptions()
                {
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}"
                );
            });
        }
    }
}