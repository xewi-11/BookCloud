using Microsoft.AspNetCore.Mvc;

namespace BookCloud.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
