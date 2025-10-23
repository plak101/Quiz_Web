using Microsoft.AspNetCore.Mvc;

namespace Quiz_Web.Controllers
{
	public class AdminController : Controller
	{
		[Route("/admin")]
		public IActionResult Index()
		{
			return View();
		}
	}
}
