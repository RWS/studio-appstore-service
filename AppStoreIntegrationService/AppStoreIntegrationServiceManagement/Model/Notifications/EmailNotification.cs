﻿using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceManagement.Model.Notifications
{
    public class EmailNotification
    {
        public EmailNotification() { }
        public EmailNotification(PluginDetails plugin)
        {
            ImageSource = plugin.Icon.MediaUrl;
            Title = plugin.Name;
            Author = plugin.Developer.DeveloperName;
        }

        public string ImageSource { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string CallToActionUrl { get; set; }
        public string Author { get; set; }

        public virtual string ToHtml()
        {
            return string.Format(TemplateResource.PluginUpdateEmailNotification, Message, ImageSource, Title, CallToActionUrl);
        }
    }
}
