using Microsoft.AspNetCore.Mvc;

namespace bookify.Web.Controllers
{
    [Authorize]
    public class DashBoardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
