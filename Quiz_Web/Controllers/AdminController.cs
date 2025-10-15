using Microsoft.AspNetCore.Mvc;

namespace Quiz_Web.Controllers
{
	public class AdminController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
