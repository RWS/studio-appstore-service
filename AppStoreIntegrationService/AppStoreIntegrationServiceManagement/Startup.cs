using System.IO.Compression;
using Microsoft.ApplicationInsights;
using AppStoreIntegrationServiceCore.Model;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.ApplicationInsights.Extensibility;
using static AppStoreIntegrationServiceCore.Enums;
using AppStoreIntegrationServiceCore.Model.Common.Interface;
using AppStoreIntegrationServiceManagement.Areas.Identity.Data;
using AppStoreIntegrationServiceCore.Repository;
using AppStoreIntegrationServiceCore.Repository.Interface;
using AppStoreIntegrationServiceManagement.Model;

namespace AppStoreIntegrationServiceManagement
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
            var settingsDeployMode = Configuration.GetValue<string>("DeployMode");
            _ = Enum.TryParse(settingsDeployMode, out DeployMode deployMode);

            var serviceProvider = services.BuildServiceProvider();
            var env = serviceProvider.GetService<IWebHostEnvironment>();

            var context = serviceProvider.GetRequiredService<AppStoreIntegrationServiceContext>();
            context.Database.EnsureCreated();

            var configurationSettings = GetConfigurationSettings(env, deployMode).Result;

            services.AddMvc();
            services.AddHttpContextAccessor();
            services.Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Optimal;
            });
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.Strict;
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
            services.AddSingleton<CustomizationHelper>();
            services.AddSingleton<INamesRepository, NamesRepository>();
            services.AddSingleton<IProductsRepository, ProductsRepository>();
            services.AddSingleton<IVersionProvider, VersionProvider>();
            services.AddSingleton<ICategoriesRepository, CategoriesRepository>();
            services.AddSingleton<ISettingsRepository, SettingsRepository>();
            services.AddSingleton<IProductsSynchronizer, ProductsSynchronizer>();
            services.AddSingleton<IConfigurationSettings>(configurationSettings);
            services.AddSingleton<IWritableOptions<SiteSettings>, WritableOptions<SiteSettings>>();
            services.Configure<SiteSettings>(options => Configuration.GetSection("SiteSettings").Bind(options));
            services.AddSingleton<IAzureRepository, AzureRepository>();
            services.AddSingleton<IPluginRepository, PluginRepository>();
            services.AddSingleton<ILocalRepository, LocalRepository>();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("IsAdmin", policy => policy.RequireRole("Administrator"));
            });

            services.AddRazorPages()
                .AddRazorPagesOptions(options => { options.Conventions.AddPageRoute("/Edit", "edit"); });
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

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCookiePolicy();
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