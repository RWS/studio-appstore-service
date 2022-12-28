using Microsoft.AspNetCore.Mvc;

namespace AppStoreIntegrationServiceManagement.Controllers.Plugins
{
    public class CommentsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
