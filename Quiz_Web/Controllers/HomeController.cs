using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quiz_Web.Models;
using Quiz_Web.Models.EF;
using Quiz_Web.Services;
using Quiz_Web.Services.IServices;

namespace Quiz_Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICourseService _courseService;
        private readonly LearningPlatformContext _context;

        public HomeController(
            ILogger<HomeController> logger, 
            ICourseService courseService,
            LearningPlatformContext context)
        {
            _logger = logger;
            _courseService = courseService;
            _context = context;
        }

        // Action mặc định để xử lý routing dựa trên authentication
        public IActionResult Welcome()
        {
            // Kiểm tra nếu user đã đăng nhập (có cookie authentication)
            if (User.Identity?.IsAuthenticated == true)
            {
                // Redirect đến trang Home/Index
                return RedirectToAction("Index");
            }
            else
            {
                // Redirect đến trang Introduce cho người dùng mới
                return RedirectToAction("Index", "Introduce");
            }
        }

        public IActionResult Index()
        {
            // Nếu người dùng chưa đăng nhập, redirect về Introduce
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction("Index", "Introduce");
            }
            
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

        // Debug action to test Onboarding view
        [Route("/debug-onboarding")]
        public async Task<IActionResult> DebugOnboarding()
        {
            try
            {
                var categories = await _context.CourseCategories
                    .OrderBy(c => c.DisplayOrder)
                    .ThenBy(c => c.Name)
                    .ToListAsync();

                var viewModel = new Quiz_Web.Models.ViewModels.OnboardingViewModel
                {
                    Categories = categories
                };

                ViewBag.Debug = $"Found {categories.Count} categories";
                
                return View("~/Views/Onboarding/Index.cshtml", viewModel);
            }
            catch (Exception ex)
            {
                return Content($"Error: {ex.Message}\n\nStack: {ex.StackTrace}");
            }
        }
  //      [Route("/course")]        
  //      public IActionResult Course()
  //      {
  //          var courses = _courseService.GetAllPublishedCourses();

		//	return View(courses);
  //      }

		//[Route("/course/{slug}")]
		//public IActionResult CourseDetail(string slug)
		//{
		//	if (string.IsNullOrEmpty(slug))
		//	{
		//		return RedirectToAction("Course");
		//	}

		//	var course = _courseService.GetCourseBySlug(slug);

		//	if (course == null)
		//	{
		//		_logger.LogWarning($"Course not found with slug: {slug}");
		//		return NotFound();
		//	}

		//	return View(course);
		//}
	}
}
