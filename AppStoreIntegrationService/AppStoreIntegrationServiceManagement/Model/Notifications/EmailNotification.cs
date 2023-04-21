using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceManagement.Model.Notifications
{
    public enum EmailType
    {
        PluginUpdate = 0,
        AccountUpdate,
        RoleUpdate,
        UserDeletion
    }

    public class EmailNotification
    {
        public const string NewProfileMessage = "A new user profile was created on RWS AppStore Manager using this email address:";
        public const string NewProfileLinked = "A user profile identified by this email address was associated to a new account in RWS AppStore Manager:";

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
        public EmailType Type { get; set; }

        public virtual string ToHtml()
        {
            return Type switch
            {
                EmailType.AccountUpdate => string.Format(TemplateResource.NewAccountEmailNotification, Message, Author, Title, CallToActionUrl),
                EmailType.PluginUpdate => string.Format(TemplateResource.PluginUpdateEmailNotification, Message, ImageSource, Title, CallToActionUrl),
                //EmailType.RoleUpdate => string.Format(TemplateResource.RoleUpdateEmailNotification, Message, Title, CallToActionUrl),
                EmailType.RoleUpdate => TemplateResource.TestEmail,
                EmailType.UserDeletion => string.Format(TemplateResource.ProfileDeleteEmailNotification, Author, Title, CallToActionUrl),
                _ => null
            };
        }
    }
}