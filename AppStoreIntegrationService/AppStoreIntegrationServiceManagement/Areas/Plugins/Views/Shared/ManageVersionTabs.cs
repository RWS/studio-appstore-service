using AppStoreIntegrationServiceCore.Model;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppStoreIntegrationServiceManagement.Areas.Plugins.Views.Shared
{
    public class ManageVersionTabs
    {
        public static string Comments => "Comments";
        public static string Details => "Details";
        public static string Draft => "Draft";
        public static string Pending => "Pending";
        public static string CommentsTabClass(ViewContext viewContext) => PageTabClass(viewContext, Comments, "ActiveVersionSubPage");
        public static string DetailsTabClass(ViewContext viewContext) => PageTabClass(viewContext, Details, "ActiveVersionSubPage");
        public static string DraftTabClass(ViewContext viewContext) => PageTabClass(viewContext, Draft, "ActiveVersionSubPage");
        public static string PendingTabClass(ViewContext viewContext) => PageTabClass(viewContext, Pending, "ActiveVersionSubPage");

        private static string PageTabClass(ViewContext viewContext, string page, string key)
        {
            var activePage = viewContext.ViewData[key] as string
                ?? Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
            return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active-tab" : null;
        }

        public static string GetAction(Status status)
        {
            return status switch
            {
                Status.Draft => "Draft",
                Status.InReview => "Pending",
                _ => "Edit"
            };
        }
    }
}
