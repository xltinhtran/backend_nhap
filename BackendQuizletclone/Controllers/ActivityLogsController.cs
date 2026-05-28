using Microsoft.AspNetCore.Mvc;

namespace BackendQuizletclone.Controllers
{
    public class ActivityLogsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
