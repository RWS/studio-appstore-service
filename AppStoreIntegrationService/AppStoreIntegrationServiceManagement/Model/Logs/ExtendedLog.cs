using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceManagement.Model.Logs
{
    public class ExtendedLog
    {
        public ExtendedLog() { }
        public ExtendedLog(PluginDetailsBase<PluginVersionBase<string>, string> plugin)
        {
            Description = "{0} added {1} at {2}.The plugin properties are:";
            Changes = new List<Change>
            {
                new Change { Name = "Plugin name", New = plugin.Name },
                new Change { Name = "Description", New = plugin.Description },
                new Change { Name = "Changelog link", New = plugin.ChangelogLink },
                new Change { Name = "Support URL", New = plugin.SupportUrl },
                new Change { Name = "Support e-mail", New = plugin.SupportEmail },
                new Change { Name = "Icon URL", New = plugin.Icon.MediaUrl },
                new Change { Name = "Pricing", New = plugin.PaidFor ? "Paid" : "Free" },
                new Change { Name = "Developer", New = plugin.Developer.DeveloperName },
                new Change { Name = "Categories", New = $"[{plugin.Categories.Aggregate("", (result, next) => result + " " + next)}]" },
                new Change { Name = "Status", New = plugin.Status.ToString() },
            };
        }

        public ExtendedLog(PluginDetailsBase<PluginVersionBase<string>, string> plugin, PluginDetailsBase<PluginVersionBase<string>, string> oldPlugin)
        {
            Changes = new List<Change>
            {
                new Change { Name = "Plugin name", New = plugin.Name, Old = oldPlugin.Name },
                new Change { Name = "Description", New = plugin.Description, Old = oldPlugin.Description },
                new Change { Name = "Changelog link", New = plugin.ChangelogLink, Old = oldPlugin.ChangelogLink },
                new Change { Name = "Support URL", New = plugin.SupportUrl, Old = oldPlugin.SupportUrl },
                new Change { Name = "Support e-mail", New = plugin.SupportEmail, Old = oldPlugin.SupportEmail },
                new Change { Name = "Icon URL", New = plugin.Icon.MediaUrl, Old = oldPlugin.Icon.MediaUrl },
                new Change { Name = "Pricing", New = plugin.PaidFor ? "Paid" : "Free", Old = oldPlugin.PaidFor ? "Paid" : "Free" },
                new Change { Name = "Developer", New = plugin.Developer.DeveloperName, Old = plugin.Developer.DeveloperName },
                new Change { Name = "Categories", New = $"[{plugin.Categories.Aggregate("", (result, next) => result + " " + next)}]", Old = $"[{oldPlugin.Categories.Aggregate("", (result, next) => result + " " + next)}]" },
                new Change { Name = "Status", New = plugin.Status.ToString(), Old = oldPlugin.Status.ToString() },
            };
        }

        public ExtendedLog(PluginVersionBase<string> version)
        {
            Changes = new List<Change>
            {
                new Change { Name = "File Hash", New = version.FileHash },
                new Change { Name = "Download URL", New = version.DownloadUrl },
                new Change { Name = "Status", New = version.VersionStatus.ToString() },
                new Change { Name = "Is private plugin", New = version.IsPrivatePlugin.ToString() },
                new Change { Name = "Is navigation link", New = version.IsNavigationLink.ToString() },
                new Change { Name = "Plugin has studio installer", New = version.AppHasStudioPluginInstaller.ToString() },
                new Change { Name = "Minimum required studio version", New = version.MinimumRequiredVersionOfStudio },
                new Change { Name = "Maximum required studio version", New = version.MaximumRequiredVersionOfStudio },
                new Change { Name = "Supported products", New = $"[{version.SupportedProducts.Aggregate("", (result, next) => result + " " + next)}]" },
            };
        }

        public ExtendedLog(PluginVersionBase<string> version, PluginVersionBase<string> oldVersion)
        {

        }

        public string Author { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public string Description { get; set; }
        public List<Change> Changes { get; set; }

        public string ToHtml()
        {
            return string.Format(Description, Author);
        }
    }
}
