using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceManagement.Model.Logs;
using AppStoreIntegrationServiceManagement.Repository.Interface;
using System.Text.RegularExpressions;

namespace AppStoreIntegrationServiceManagement.Repository
{
    public class LoggingRepository : ILoggingRepository
    {
        private readonly IResponseManager _responseManager;
        private readonly ICategoriesRepository _categoriesRepository;
        private readonly IProductsRepository _productsRepository;

        public LoggingRepository
        (
            IResponseManager responseManager, 
            ICategoriesRepository categoriesRepository,
            IProductsRepository productsRepository
        )
        {
            _responseManager = responseManager;
            _categoriesRepository = categoriesRepository;
            _productsRepository = productsRepository;
        }

        public async Task<IEnumerable<Log>> GetPluginLogs(int pluginId)
        {
            var logs = await GetAllLogs();
            return logs.TryGetValue(pluginId, out var pluginLogs) ? pluginLogs : Enumerable.Empty<Log>();
        }

        public async Task Log(string username, int pluginId, string custom)
        {
            if (string.IsNullOrEmpty(custom))
            {
                return;
            }

            await Save(new Log
            {
                Author = username,
                Date = DateTime.Now,
                Description = custom
            }, pluginId);
        }

        public IEnumerable<Log> SearchLogs(IEnumerable<Log> logs, DateTime from, DateTime to, string query = null)
        {
            var filtered = logs.Where(x => x.Date >= from && x.Date <= to);
            if (string.IsNullOrEmpty(query))
            {
                return filtered;
            }

            return filtered.Where(x => Regex.IsMatch(x.Description, query, RegexOptions.IgnoreCase));
        }

        public async Task ClearLogs(int pluginId)
        {
            var logs = await GetAllLogs();

            if (logs.ContainsKey(pluginId))
            {
                logs[pluginId] = new List<Log>();
            }

            await UpdateLogs(logs);
        }

        public string CreateChangesLog(PluginDetails latest, PluginDetails old, string username)
        {
            var oldToBase = PluginDetailsBase<PluginVersionBase<string>, string>.CopyFrom(old);
            var newToBase = PluginDetailsBase<PluginVersionBase<string>, string>.CopyFrom(latest);

            if (oldToBase == null)
            {
                return string.Format(TemplateResource.NewPluginLog, username, latest.Name, DateTime.Now, CreateNewLog(newToBase));
            }

            if (oldToBase.Equals(newToBase))
            {
                return null;
            }

            return string.Format(TemplateResource.PluginUpdateLog, username, latest.Name, DateTime.Now, CreateComparisonLog(newToBase, oldToBase));
        }

        private async Task Save(Log log, int pluginId)
        {
            var logs = await GetAllLogs();

            if (logs.TryGetValue(pluginId, out var pluginLogs))
            {
                logs[pluginId] = pluginLogs.Append(log);
            }
            else
            {
                logs.Add(pluginId, new List<Log> { log });
            }

            await UpdateLogs(logs);
        }

        private async Task<IDictionary<int, IEnumerable<Log>>> GetAllLogs()
        {
            var data = await _responseManager.GetResponse();
            return data.Logs;
        }

        private async Task UpdateLogs(IDictionary<int, IEnumerable<Log>> logs)
        {
            var data = await _responseManager.GetResponse();
            data.Logs = logs;
            await _responseManager.SaveResponse(data);
        }

        private string CreateNewLog(PluginDetailsBase<PluginVersionBase<string>, string> plugin)
        {
            string change = "<li><b>{0}</b> : <i>{1}</i></li>";
            return string.Format(change, "Plugin name", plugin.Name) +
                   string.Format(change, "Description", plugin.Description) +
                   string.Format(change, "Changelog link", plugin.ChangelogLink) +
                   string.Format(change, "Support URL", plugin.SupportUrl) +
                   string.Format(change, "Support e-mail", plugin.SupportEmail) +
                   string.Format(change, "Icon URL", plugin.Icon.MediaUrl) +
                   string.Format(change, "Pricing", plugin.PaidFor ? "Paid" : "Free") +
                   string.Format(change, "Developer", plugin.Developer.DeveloperName) +
                   string.Format(change, "Categories", CreateCategoriesLog(plugin.Categories)) +
                   string.Format(change, "Status", plugin.Status.ToString());
        }

        private string CreateComparisonLog(PluginDetailsBase<PluginVersionBase<string>, string> latest, PluginDetailsBase<PluginVersionBase<string>, string> old)
        {
            string change = "<li>The property <b>{0}</b> changed from <i>{1}</i> to <i>{2}</i></li>";
            return (latest.Name == old.Name ? null : string.Format(change, "Plugin name", latest.Name, old.Name)) +
                   (latest.Status == old.Status ? null : string.Format(change, "Status", latest.Status, old.Status)) +
                   (latest.PaidFor == old.PaidFor ? null : string.Format(change, "Pricing", latest.PaidFor, old.PaidFor)) +
                   (latest.Icon.Equals(old.Icon) ? null : string.Format(change, "Icon URL", latest.Icon.MediaUrl, old.Icon.MediaUrl)) +
                   (latest.SupportUrl == old.SupportUrl ? null : string.Format(change, "Support URL", latest.SupportUrl, old.SupportUrl)) +
                   (latest.Description == old.Description ? null : string.Format(change, "Description", latest.Description, old.Description)) +
                   (latest.SupportEmail == old.SupportEmail ? null : string.Format(change, "Support e-mail", latest.SupportEmail, old.SupportEmail)) +
                   (latest.ChangelogLink == old.ChangelogLink ? null : string.Format(change, "Changelog link", latest.ChangelogLink, old.ChangelogLink)) +
                   (latest.Developer.Equals(old.Developer) ? null : string.Format(change, "Developer", latest.Developer.DeveloperName, old.Developer.DeveloperName)) +
                   (latest.Categories.SequenceEqual(old.Categories) ? null : string.Format(change, "Categories", CreateCategoriesLog(latest.Categories), CreateCategoriesLog(@old.Categories)));
        }

        private string CreateCategoriesLog(List<string> categories)
        {
            var categoryDetails = _categoriesRepository.GetAllCategories().Result;
            if (categories.Count > 1)
            {
                return $"[{categories.Aggregate("", (result, next) => $"{result}, {categoryDetails.FirstOrDefault(c => c.Id == next).Name}")}]";
            }

            return $"[{categoryDetails.FirstOrDefault(c => c.Id == categories[0]).Name}]";
        }

        public string CreateChangesLog(PluginVersion @new, PluginVersion old, string username)
        {
            var oldToBase = PluginVersionBase<string>.CopyFrom(old);
            var newToBase = PluginVersionBase<string>.CopyFrom(@new);

            if (oldToBase == null)
            {
                return $"<b>{username}</b> added version with number {@new.VersionNumber} at {DateTime.Now}<br><br><p>The version properties are:</p><ul>{CreateNewLog(newToBase)}</ul>";
            }

            if (oldToBase.Equals(newToBase))
            {
                return null;
            }

            return $"<b>{username}</b> made changes to the version with number {@new.VersionNumber} at {DateTime.Now}<br><br><p>The following changes occured:</p><ul>{CreateComparisonLog(newToBase, oldToBase)}</ul>";
        }

        private string CreateNewLog(PluginVersionBase<string> version)
        {
            string change = "<li><b>{0}</b> : <i>{1}</i></li>";
            return string.Format(change, "File hash", version.FileHash) +
                   string.Format(change, "Download URL", version.DownloadUrl) +
                   string.Format(change, "Status", version.VersionStatus.ToString()) +
                   string.Format(change, "Is private plugin", version.IsPrivatePlugin) +
                   string.Format(change, "Is navigation link", version.IsNavigationLink) +
                   string.Format(change, "Supported products", CreateProductsLog(version.SupportedProducts)) +
                   string.Format(change, "Plugin has studio installer", version.AppHasStudioPluginInstaller) +
                   string.Format(change, "Minimum required studio version", version.MinimumRequiredVersionOfStudio) +
                   string.Format(change, "Maximum required studio version", version.MaximumRequiredVersionOfStudio);
        }

        private string CreateComparisonLog(PluginVersionBase<string> @new, PluginVersionBase<string> old)
        {
            string change = "<li>The property <b>{0}</b> changed from <i>{1}</i> to <i>{2}</i></li>";
            return (@new.FileHash == old.FileHash ? null : string.Format(change, "File hash", @new.FileHash, old.FileHash)) +
                   (@new.DownloadUrl == old.DownloadUrl ? null : string.Format(change, "Download URL", @new.DownloadUrl, old.DownloadUrl)) +
                   (@new.VersionStatus == old.VersionStatus ? null : string.Format(change, "Status", @new.VersionStatus, old.VersionStatus)) +
                   (@new.IsPrivatePlugin == old.IsPrivatePlugin ? null : string.Format(change, "Is private plugin", @new.IsPrivatePlugin, old.IsPrivatePlugin)) +
                   (@new.IsNavigationLink == old.IsNavigationLink ? null : string.Format(change, "Is navigation link", @new.IsNavigationLink, old.IsNavigationLink)) +
                   (@new.AppHasStudioPluginInstaller == old.AppHasStudioPluginInstaller ? null : string.Format(change, "Plugin has studio installer", @new.AppHasStudioPluginInstaller, old.AppHasStudioPluginInstaller)) +
                   (@new.SupportedProducts.SequenceEqual(old.SupportedProducts) ? null : string.Format(change, "Supported products", CreateProductsLog(@new.SupportedProducts), CreateProductsLog(@old.SupportedProducts))) +
                   (@new.MinimumRequiredVersionOfStudio == old.MinimumRequiredVersionOfStudio ? null : string.Format(change, "Minimum required studio version", @new.MinimumRequiredVersionOfStudio, old.MinimumRequiredVersionOfStudio)) +
                   (@new.MaximumRequiredVersionOfStudio == old.MaximumRequiredVersionOfStudio ? null : string.Format(change, "Maximum required studio versionr", @new.MaximumRequiredVersionOfStudio, old.MaximumRequiredVersionOfStudio));
        }

        private string CreateProductsLog(List<string> products)
        {
            var productDetails = _productsRepository.GetAllProducts().Result;
            if (products.Count > 1)
            {
                return $"[{products.Aggregate("", (result, next) => $"{result}, {productDetails.FirstOrDefault(c => c.Id == next).ProductName}")}]";
            }

            return $"[{productDetails.FirstOrDefault(c => c.Id == products[0]).ProductName}]";
        }
    }
}
