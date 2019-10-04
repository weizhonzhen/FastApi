using Microsoft.AspNetCore.Mvc;

namespace FastApiGatewayDb.Ui.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
