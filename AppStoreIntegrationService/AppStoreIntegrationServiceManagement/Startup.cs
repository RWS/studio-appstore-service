using System.IO.Compression;
using Microsoft.AspNetCore.ResponseCompression;
using static AppStoreIntegrationServiceCore.Enums;
using AppStoreIntegrationServiceManagement.Areas.Identity.Data;
using AppStoreIntegrationServiceCore.Repository;
using AppStoreIntegrationServiceCore.Repository.Interface;
using AppStoreIntegrationServiceCore.Model.Common.Interface;
using AppStoreIntegrationServiceCore.Model;
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
            _ = Enum.TryParse(Configuration.GetValue<string>("DeployMode"), out DeployMode deployMode);
            GetServiceProvider(services).GetRequiredService<AppStoreIntegrationServiceContext>().Database.EnsureCreated();

            services.Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Optimal;
            });

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.Strict;
            });

            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<GzipCompressionProvider>();
            });

            services.AddMvc();
            services.AddHttpContextAccessor();
            services.AddResponseCaching();
            services.AddHttpContextAccessor();

            if (deployMode == DeployMode.AzureBlob)
            {
                services.AddSingleton<IResponseManager, AzureRepository>();
                services.AddSingleton<IPluginManager, AzureRepository>();
                services.AddSingleton<IProductsManager, AzureRepository>();
                services.AddSingleton<IVersionManager, AzureRepository>();
                services.AddSingleton<INamesManager, AzureRepository>();
                services.AddSingleton<ICategoriesManager, AzureRepository>();
                services.AddSingleton<ISettingsManager, AzureRepository>();
                services.AddSingleton<ICommentsManager, AzureRepository>();
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
                services.AddSingleton<ICommentsManager, LocalRepository>();
                services.AddSingleton<IWritableOptions<SiteSettings>, WritableOptions<SiteSettings>>();
                services.Configure<SiteSettings>(options => Configuration.GetSection("SiteSettings").Bind(options));
            }

            services.AddSingleton<IConfigurationSettings>(GetConfigurationSettings(GetServiceProvider(services).GetService<IWebHostEnvironment>(), deployMode).Result);
            services.AddSingleton<ICategoriesRepository, CategoriesRepository>();
            services.AddSingleton<IPluginRepository, PluginRepository>();
            services.AddSingleton<IProductsRepository, ProductsRepository>();
            services.AddSingleton<INamesRepository, NamesRepository>();
            services.AddSingleton<ICommentsRepository, CommentsRepository>();
            services.AddSingleton<CustomizationHelper>();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("IsAdmin", policy => policy.RequireRole("Administrator"));
            });

            services.AddRazorPages().AddRazorPagesOptions(options => { options.Conventions.AddPageRoute("/Edit", "edit"); });
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