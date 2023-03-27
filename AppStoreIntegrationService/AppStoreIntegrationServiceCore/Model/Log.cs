namespace AppStoreIntegrationServiceCore.Model
{
    public class Log
    {
        public Log() { }

        public Log(PluginDetails plugin, string username)
        {
            TargetInfo = plugin.Name;
            Author = username;
            Description = "<b>{0}</b> added {1} at {2}.<br><br> <p>The plugin properties are:</p>";
            IsNewLog = true;
            Changes = new List<Change>
            {
                new Change { Name = "Plugin name", New = plugin.Name },
                new Change { Name = "Description", New = plugin.Description[..^ 3][3 ..] },
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

        public Log(PluginDetails plugin, PluginDetails oldPlugin, string username) : this(plugin, username)
        {
            if (oldPlugin == null)
            {
                return;
            }

            IsNewLog = false;
            TargetInfo = plugin.Name;
            Author = username;
            Description = "<b>{0}</b> made changes to the plugin with name {1} at {2}.<br><br> <p>The following changes occured:</p>";
            Changes = new List<Change>
            {
                new Change { Name = "Plugin name", New = plugin.Name, Old = oldPlugin.Name },
                new Change { Name = "Description", New = plugin.Description[..^3][3..], Old = oldPlugin.Description[..^3][3..] },
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

        public Log(PluginVersion version, string username)
        {
            TargetInfo = version.VersionNumber;
            Author = username;
            Description = "<b>{0}</b> added version with number {1} at {2}.<br><br> <p>The version properties are:</p>";
            IsNewLog = true;
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

        public Log(PluginVersion version, PluginVersion oldVersion, string username) : this(version, username)
        {
            if (oldVersion == null)
            {
                return;
            }

            IsNewLog = false;
            TargetInfo = version.VersionNumber;
            Author = username;
            Description = "<b>{0}</b> made changes to the version with number {1} at {2}.<br><br> <p>The following changes occured:</p>";
            Changes = new List<Change>
            {
                new Change { Name = "FileHash", New = version.FileHash, Old = oldVersion.FileHash },
                new Change { Name = "Download URL", New = version.DownloadUrl, Old = oldVersion.DownloadUrl },
                new Change { Name = "Status", New = version.VersionStatus.ToString(), Old = oldVersion.VersionStatus.ToString() },
                new Change { Name = "Is private plugin", New = version.IsPrivatePlugin.ToString(), Old = oldVersion.IsPrivatePlugin.ToString() },
                new Change { Name = "Is navigation link", New = version.IsNavigationLink.ToString(), Old = oldVersion.IsNavigationLink.ToString() },
                new Change { Name = "Plugin has studio installer", New = version.AppHasStudioPluginInstaller.ToString(), Old = oldVersion.AppHasStudioPluginInstaller.ToString() },
                new Change { Name = "Minimum required studio version", New = version.MinimumRequiredVersionOfStudio, Old = oldVersion.MinimumRequiredVersionOfStudio },
                new Change { Name = "Maximum required studio version", New = version.MaximumRequiredVersionOfStudio, Old = oldVersion.MaximumRequiredVersionOfStudio },
                new Change { Name = "Supported products", New = $"[{version.SupportedProducts.Aggregate("", (result, next) => result + " " + next)}]", Old = $"[{oldVersion.SupportedProducts.Aggregate("", (result, next) => result + " " + next)}]" },
            };
        }

        public string Author { get; set; }
        public bool IsNewLog { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public string Description { get; set; }
        public string TargetInfo { get; set; }
        public List<Change> Changes { get; set; } = new List<Change>();

        public string ToHtml() => string.Format(Description, Author, TargetInfo, Date) + GetDetails();

        private string GetDetails(string change = null)
        {
            var changes = "<ul>";
            if (IsNewLog)
            {
                change = "<li><b>{0}</b> : <i>{1}</i></li>";
                return Changes.Aggregate(changes, (current, next) => current + string.Format(change, next.Name, next.New)) + "</ul>";
            }

            change = "<li>The property <b>{0}</b> changed from <i>{1}</i> to <i>{2}</i></li>";
            foreach (var item in Changes)
            {
                if (item.New == item.Old)
                {
                    continue;
                }

                changes += string.Format(change, item.Name, item.Old, item.New);
            }

            return changes + "</ul>";
        }
    }
}
