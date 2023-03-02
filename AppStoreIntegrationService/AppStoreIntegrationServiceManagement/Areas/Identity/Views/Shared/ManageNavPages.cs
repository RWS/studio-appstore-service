using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppStoreIntegrationServiceManagement.Areas.Identity.Views.Shared
{
    public static class ManageNavPages
    {
        public static string Index => "Index";
        public static string Register => "Register";
        public static string ChangePassword => "ChangePassword";
        public static string Manage => "Manage";
        public static string Edit => "Edit";
        public static string Notifications => "Notifications";
        public static string Accounts => "Accounts";
        public static string ProfileNavClass(ViewContext viewContext) => PageNavClass(viewContext, Index);
        public static string RegisterNavClass(ViewContext viewContext) => PageNavClass(viewContext, Register);
        public static string ChangePasswordNavClass(ViewContext viewContext) => PageNavClass(viewContext, ChangePassword);
        public static string ManageNavClass(ViewContext viewContext) => PageNavClass(viewContext, Manage);
        public static string EditUserNavClass(ViewContext viewContext) => PageNavClass(viewContext, Edit);
        public static string NotificationsNavClass(ViewContext viewContext) => PageNavClass(viewContext, Notifications);
        public static string AccountsNavClass(ViewContext viewContext) => PageNavClass(viewContext, Accounts);

        private static string PageNavClass(ViewContext viewContext, string page)
        {
            var activePage = viewContext.ViewData["ActivePage"] as string
                ?? Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
            return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
        }
    }
}
