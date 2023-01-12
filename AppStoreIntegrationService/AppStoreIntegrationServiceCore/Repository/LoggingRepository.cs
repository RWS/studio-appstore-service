using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using System.Reflection;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace AppStoreIntegrationServiceCore.Repository
{
    public class LoggingRepository : ILoggingRepository
    {
        private readonly ILogsManager _logsManager;
        private readonly ICategoriesRepository _categoriesRepository;
        private readonly IProductsRepository _productsRepository;

        public LoggingRepository(ILogsManager logsManager, ICategoriesRepository categoriesRepository, IProductsRepository productsRepository)
        {
            _logsManager = logsManager;
            _categoriesRepository = categoriesRepository;
            _productsRepository = productsRepository;
        }

        public async Task<IEnumerable<Log>> GetPluginLogs(int pluginId)
        {
            var logs = await _logsManager.ReadLogs();
            return logs.TryGetValue(pluginId, out var pluginLogs) ? pluginLogs : Enumerable.Empty<Log>();
        }

        public async Task Log(ClaimsPrincipal user, PluginDetailsBase<PluginVersionBase<string>, string> current, PluginDetailsBase<PluginVersionBase<string>, string> old = null)
        {
            var description = string.Empty;
            if (Equals(old, null))
            {
                description = $"<b>{user.Identity.Name} added {current.Name} at {DateTime.Now}</b>";
            }
            else
            {
                var properties = typeof(PluginDetailsBase<PluginVersionBase<string>, string>).GetProperties().Where(p => !p.Name.Equals("Versions"));
                var change = properties.Aggregate(new List<string>(), (changes, next) => changes.Append(GetPluginChange(old, current, next)).ToList()).Aggregate((current, next) => current + next);
                description = string.Format(ServiceResource.NewPluginLogString, user.Identity.Name, old.Name, DateTime.Now, change);
            }
            
            await Save(new Log
            {
                Author = user.Identity.Name,
                Date = DateTime.Now,
                Description = description
            }, old.Id);
        }

        public async Task Log(ClaimsPrincipal user, int pluginId, PluginVersionBase<string> current, PluginVersionBase<string> old = null)
        {
            var description = string.Empty;
            if (Equals(old, null))
            {
                description = $"<b>{user.Identity.Name}</b> added the version with number <b>{current.VersionNumber}</b> at {DateTime.Now}";
            }
            else
            {
                var properties = typeof(PluginVersionBase<string>).GetProperties();
                var change = properties.Aggregate(new List<string>(), (changes, next) => changes.Append(GetVersionChange(old, current, next)).ToList()).Aggregate((current, next) => current + next);
                description = string.Format(ServiceResource.NewVersionLogString, user.Identity.Name, DateTime.Now, old.VersionNumber, change);
            }

            await Save(new Log
            {
                Author = user.Identity.Name,
                Date = DateTime.Now,
                Description = description
            }, pluginId);
        }

        public async Task Log(ClaimsPrincipal user, int pluginId, string custom)
        {
            await Save(new Log
            {
                Author = user.Identity.Name,
                Date = DateTime.Now,
                Description = custom
            }, pluginId);
        }

        private string GetVersionChange(PluginVersionBase<string> old, PluginVersionBase<string> current, PropertyInfo info)
        {
            return info.Name switch
            {
                "SupportedProducts" => GetProductsChange(old, current),
                _ => GetPropertyChange(old, current, info)
            };
        }

        private string GetProductsChange(PluginVersionBase<string> old, PluginVersionBase<string> current)
        {
            if (old.SupportedProducts.SequenceEqual(current.SupportedProducts))
            {
                return null;
            }

            var products = _productsRepository.GetAllProducts().Result;
            var oldValue = products.Where(x => old.SupportedProducts.Any(y => y.Equals(x.Id))).Select(x => x.ProductName).Aggregate((current, next) => $"{current}, {next}");
            var newValue = products.Where(x => current.SupportedProducts.Any(y => y.Equals(x.Id))).Select(x => x.ProductName).Aggregate((current, next) => $"{current}, {next}");
            return string.Format(ServiceResource.ArrayLogString, "Supported Products", oldValue, newValue);
        }

        private string GetPluginChange(PluginDetailsBase<PluginVersionBase<string>, string> old, PluginDetailsBase<PluginVersionBase<string>, string> current, PropertyInfo info)
        {
            return info.Name switch
            {
                "Categories" => GetCategoriesChange(old, current),
                "Icon" => old.Icon.Equals(current.Icon) ? null : string.Format(ServiceResource.LogString, "Icon url", old.Icon.MediaUrl, current.Icon.MediaUrl),
                "Developer" => old.Developer.Equals(current.Developer) ? null : string.Format(ServiceResource.LogString, "Developer name", old.Developer.DeveloperName, old.Developer.DeveloperName),
                _ => GetPropertyChange(old, current, info)
            };
        }

        private string GetPropertyChange(object old, object current, PropertyInfo info)
        {
            var oldValue = info.GetValue(old);
            var newValue = info.GetValue(current);

            if (oldValue.Equals(newValue))
            {
                return null;
            }
            string name = (string)info.CustomAttributes?.FirstOrDefault(a => a.AttributeType.Name.Equals("DisplayAttribute"))?.NamedArguments?
                                                        .FirstOrDefault(n => n.MemberName.Equals("Name")).TypedValue.Value;
            return string.Format(ServiceResource.LogString, name, oldValue, newValue);
        }

        private string GetCategoriesChange(PluginDetailsBase<PluginVersionBase<string>, string> old, PluginDetailsBase<PluginVersionBase<string>, string> current)
        {
            if (old.Categories.SequenceEqual(current.Categories))
            {
                return null;
            }

            var categories = _categoriesRepository.GetAllCategories().Result;
            var oldValue = categories.Where(x => old.Categories.Any(y => y.Equals(x.Id))).Select(x => x.Name).Aggregate((current, next) => $"{current}, {next}");
            var newValue = categories.Where(x => current.Categories.Any(y => y.Equals(x.Id))).Select(x => x.Name).Aggregate((current, next) => $"{current}, {next}");
            return string.Format(ServiceResource.ArrayLogString, "Categories", oldValue, newValue);
        }

        private async Task Save(Log log, int pluginId)
        {
            var logs = await _logsManager.ReadLogs();

            if (logs.TryGetValue(pluginId, out var pluginLogs))
            {
                logs[pluginId] = pluginLogs.Append(log);
            }
            else
            {
                logs.Add(pluginId, new List<Log> { log });
            }

            await _logsManager.UpdateLogs(logs);
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
    }
}
