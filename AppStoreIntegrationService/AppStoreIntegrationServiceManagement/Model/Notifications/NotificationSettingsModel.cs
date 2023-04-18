using AppStoreIntegrationServiceCore.DataBase.Models;

namespace AppStoreIntegrationServiceManagement.Model.Notifications
{
    public class NotificationSettingsModel
    {
        public NotificationSettingsModel() { }

        public NotificationSettingsModel(UserProfile user)
        {
            EmailNotificationsEnabled = user.EmailNotificationsEnabled;
            PushNotificationsEnabled = user.PushNotificationsEnabled;
        }

        public bool EmailNotificationsEnabled { get; set; }
        public bool PushNotificationsEnabled { get; set; }
    }
}
