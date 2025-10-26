using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Quiz_Web.Models;
using Quiz_Web.Services;
using Quiz_Web.Services.IServices;

namespace Quiz_Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICourseService _courseService;

        public HomeController(ILogger<HomeController> logger, ICourseService courseService)
        {
            _logger = logger;
            _courseService = courseService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [Route("/course")]        
        public IActionResult Course()
        {
            var courses = _courseService.GetAllPublishedCourses();

			return View(courses);
        }
    }
}
