using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Quiz_Web.Helper;
using Quiz_Web.Models.Entities;
using Quiz_Web.Services.IServices;
using Quiz_Web.Utils;
using System.Security.Claims;

namespace Quiz_Web.Controllers
{
	public class AccountController : Controller
	{
		private readonly IUserService _userService;
		public AccountController(IUserService userService)
		{
			_userService = userService;
		}

		public IActionResult Index()
		{
			return View();
		}
		[Route("/login")]
		public IActionResult Login()
		{
			if (User.Identities != null && User.Identity.IsAuthenticated)
			{
				return Redirect("/admin/Index");
			}
			return View();
		}
		// POST: /Account/Login

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<JsonResult> LoginToSystem(string username, string password)
		{
			try
			{
				var user = _userService.Login(username, HashHelper.ComputeHash(password));
				if (user != null)
				{
					var claims = new List<Claim>();
					claims.Add(new Claim(ClaimTypes.Name, user.FullName));
					claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Username));
					claims.Add(new Claim(ClaimTypes.Role, user.Role?.Name ?? string.Empty)); // Fix: null-safe access

					var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
					await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

					return Json(new { status = WebConstants.SUCCESS });
				}
				else
				{
					return Json(new { status = WebConstants.ERROR, message = "Tài khoản hoặc mật khẩu không chính xác."});
				}
			}
			catch (Exception ex)
			{
				return Json(new { status = WebConstants.ERROR, message = "Lỗi đăng nhập", error = ex.ToString() });
			}
		}

		[Route("/register")]
		public IActionResult Register()
		{

			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<JsonResult> RegisterToSystem(string fullname, string email, string username, string password, string confirmPassword)
		{
			try
			{
				if (!Validation.IsValidEmail(email)) return Json(new { status = WebConstants.ERROR, message = "Email không đúng định dạng" });
				if (!Validation.IsValidUsername(username)) return Json(new { status = WebConstants.ERROR, message = "Tên đăng nhập phải có ít nhất 6 ký tự" });
				if (!Validation.IsValidPassword(password)) return Json(new { status = WebConstants.ERROR, message = "Mật khẩu phải có ít nhất 8 ký tự, bao gồm chữ hoa, chữ thường, số và ký tự đặc biệt" });
				if (_userService.ExistsEmail(email)) return Json(new { status = WebConstants.ERROR, message = "Email đã được sử dụng" });
				if (_userService.ExistsUsername(username)) return Json(new { status = WebConstants.ERROR, message = "Tên đăng nhập đã được sử dụng" });
				if (confirmPassword.Equals(password) == false) return Json(new { status = WebConstants.ERROR, message = "Mật khẩu xác nhận không khớp" });

				var user = new User
				{
					Email = email.ToLower().Trim(),
					Username = username.ToLower().Trim(),
					PasswordHash = HashHelper.ComputeHash(password.Trim()),
					FullName = fullname.Trim(),
					RoleId = 3
				};

				if (_userService.Register(user))
				{
					return Json(new { status = WebConstants.SUCCESS });
				}
				else
				{
					return Json(new { status = WebConstants.ERROR, message = "Lỗi khi tạo tài khoản" });
				}
			}
			catch (Exception ex)
			{
				return Json(new { status = WebConstants.ERROR, message = "Lỗi khi đăng ký", error = ex.ToString() });
			}
		}
		public IActionResult ForgotPassword()
		{
			return View();
		}
	}

}
