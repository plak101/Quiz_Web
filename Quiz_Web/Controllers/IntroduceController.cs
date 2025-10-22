using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Quiz_Web.Controllers
{
    public class IntroduceController : Controller
    {
        private readonly ILogger<IntroduceController> _logger;

        public IntroduceController(ILogger<IntroduceController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("/admin/Introduce")]
        public IActionResult Index()
        {
            return View("~/Views/Introduce.cshtml");
        }
    }
}
