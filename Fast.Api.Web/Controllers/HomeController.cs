using Microsoft.AspNetCore.Mvc;

namespace FastApiGatewayDb.Ui.Controllers
{
    public class HomeController : Controller
    {
        [Route("help")]
        public IActionResult Index()
        {
            return View();
        }
        [Route("xml")]
        public IActionResult Xml()
        {
            return View();
        }
    }
}
