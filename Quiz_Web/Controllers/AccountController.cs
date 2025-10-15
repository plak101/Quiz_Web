using Microsoft.AspNetCore.Mvc;

namespace Quiz_Web.Controllers
{
	public class AccountController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}

		public IActionResult Login()
		{
			return View();
		}
		// POST: /Account/Login
		[HttpPost]
		public IActionResult Login(string username, string password)
		{
			if (username == "admin" && password == "123")
				return RedirectToAction("Index", "Home");

			ViewBag.Error = "Sai tên đăng nhập hoặc mật khẩu!";
			return View();
		}

		public IActionResult Register()
		{
			return View();
		}

		[HttpPost]
		public IActionResult Register(string email, string username, string password)
		{
			// TODO: Lưu thông tin đăng ký vào DB
			ViewBag.Success = "Đăng ký thành công!";
			return View();
		}
	}

}
