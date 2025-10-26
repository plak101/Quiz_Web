using Microsoft.AspNetCore.Mvc;

namespace Quiz_Web.Controllers
{
    public class TextController : Controller
    {
        [HttpGet]
        [Route("/admin/Text")]
        public IActionResult Text()
        {
            return View("~/Views/Text/Text.cshtml");
        }
    }
}
