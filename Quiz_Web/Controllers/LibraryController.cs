using Microsoft.AspNetCore.Mvc;

namespace Quiz_Web.Controllers
{
    public class LibraryController : Controller
    {
        [HttpGet]
        [Route("/admin/Library")]
        public IActionResult Library()
        {
            return View("~/Views/Library/Library.cshtml");
        }

        public IActionResult AllCourses()
        {
            // TODO: Fetch all courses from service/database
            return PartialView("_AllCourses");
        }

        public IActionResult Wishlist()
        {
            // TODO: Fetch wishlist from service/database
            return PartialView("_Wishlist");
        }
    }
}
