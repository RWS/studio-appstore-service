using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;
using System.Net;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using System.Net.Http.Headers;
using static AppStoreIntegrationServiceCore.Enums;
using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceManagement.Areas.Identity.Data;
using AppStoreIntegrationServiceCore.Repository.Interface;
using AppStoreIntegrationServiceCore.Repository.Common;
using AppStoreIntegrationServiceCore.Repository.V2.Interface;
using AppStoreIntegrationServiceCore.Repository.V2;
using AppStoreIntegrationServiceCore.Repository.Common.Interface;
using AppStoreIntegrationServiceCore.Model.Common.Interface;

namespace AppStoreIntegrationServiceManagement
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //Read the deploy mode from appsettings.json
            var settingsDeployMode = Configuration.GetValue<string>("DeployMode");
            _ = Enum.TryParse(settingsDeployMode, out DeployMode deployMode);

            //Read the web host enviroment in case the service is set on LocalFilePath
            //we'll need to read the json file with the plugins details from local path
            var serviceProvider = services.BuildServiceProvider();
            var env = serviceProvider.GetService<IWebHostEnvironment>();

            var context = serviceProvider.GetRequiredService<AppStoreIntegrationServiceContext>();
            context.Database.EnsureCreated();

            var configurationSettings = GetConfigurationSettings(env, deployMode).Result;
            ConfigureHttpClient(services);

            services.AddMvc();

            services.AddHttpContextAccessor();
            services.Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Optimal;
            });

            if (!string.IsNullOrEmpty(configurationSettings.InstrumentationKey))
            {
                services.AddApplicationInsightsTelemetry();
                new TelemetryClient(new TelemetryConfiguration
                {
                    ConnectionString = configurationSettings.InstrumentationKey
                }).TrackEvent("Application started");
            }

            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<GzipCompressionProvider>();
            });

            services.AddResponseCaching();
            services.AddHttpContextAccessor();
            services.AddSingleton<IAzureRepositoryExtended<PluginDetails<PluginVersion<string>>>, AzureRepositoryExtended<PluginDetails<PluginVersion<string>>>>();
            services.AddSingleton<IProductsSynchronizer, ProductsSynchronizer>();
            services.AddSingleton<INamesRepository, NamesRepository>();
            services.AddSingleton<IProductsRepository, ProductsRepository>();
            services.AddSingleton<IConfigurationSettings>(configurationSettings);
            services.AddSingleton<IWritableOptions<SiteSettings>, WritableOptions<SiteSettings>>();
            services.Configure<SiteSettings>(options => Configuration.GetSection("SiteSettings").Bind(options));
            services.AddSingleton<ISettingsRepository, SettingsRepository>();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("IsAdmin", policy => policy.RequireRole("Administrator"));
            });

            services.AddRazorPages()
                .AddRazorPagesOptions(options => { options.Conventions.AddPageRoute("/Edit", "edit"); });
        }

        private static void ConfigureHttpClient(IServiceCollection services)
        {
            services.AddHttpClient("HttpClientWithSSLUntrusted").ConfigurePrimaryHttpMessageHandler(() =>
                new HttpClientHandler
                {
                    ClientCertificateOptions = ClientCertificateOption.Manual,
                    ServerCertificateCustomValidationCallback =
                        (httpRequestMessage, cert, cetChain, policyErrors) => true
                });

            services.AddHttpClient<IPluginRepositoryExtended<PluginDetails<PluginVersion<string>>>, PluginRepositoryExtended<PluginDetails<PluginVersion<string>>>>(l =>
            {
                l.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                l.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
                l.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
                l.DefaultRequestHeaders.Connection.Add("Keep-Alive");
                l.DefaultRequestHeaders.Add("Connection", "Keep-Alive");
                l.DefaultRequestHeaders.TransferEncodingChunked = false;
                l.Timeout = TimeSpan.FromMinutes(5);
            }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            });
        }

        private async Task<ConfigurationSettings> GetConfigurationSettings(IWebHostEnvironment env, DeployMode deployMode)
        {
            var configurationSettings = new ConfigurationSettings(deployMode);
            //Read from appsettings if exists 
            var section = Configuration.GetSection("ConfigurationSettings");
            //Bind pre-defined properties
            if (section.Exists())
            {
                Configuration.Bind("ConfigurationSettings", configurationSettings);
            }
            else
            {
                //Read from environment variables
                configurationSettings.LoadVariables();
            }
            await configurationSettings.SetFilePathsProperties(env);

            return configurationSettings;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseResponseCaching();

            app.Use(async (context, next) =>
            {
                context.Response.GetTypedHeaders().CacheControl =
                    new Microsoft.Net.Http.Headers.CacheControlHeaderValue()
                    {
                        Public = true,
                        MaxAge = TimeSpan.FromMinutes(9),

                    };
                context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.Vary] =
                    new[] { "Accept-Encoding" };

                await next();
            });

            app.UseResponseCompression();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
                );
            });
        }
    }
}