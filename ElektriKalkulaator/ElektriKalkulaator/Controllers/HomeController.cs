using Microsoft.AspNetCore.Mvc;

namespace ElektriKalkulaator.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // GET / — landing page with hero section
        public IActionResult Index() => View();

        // GET /Home/Privacy
        public IActionResult Privacy() => View();

        // GET /Home/Error — shown when an unhandled exception occurs
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View();
    }
}
