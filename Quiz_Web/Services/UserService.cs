using Microsoft.EntityFrameworkCore;
using Quiz_Web.Models.EF;
using Quiz_Web.Models.Entities;
using Quiz_Web.Services.IServices;

namespace Quiz_Web.Services
{
	public class UserService : IUserService
	{
		private readonly LearningPlatformContext _context;
		public UserService(LearningPlatformContext context)
		{
			_context = context;
		}



		public User Login(string username, string password)
		{
			try
			{
				var user = _context.Users.
					Include(u => u.Role).
					FirstOrDefault(u => u.Username == username.ToLower().Trim() && u.PasswordHash == password.ToLower().Trim());
				return user;
			}
			catch (Exception ex)
			{
				return null;
			}
		}

		public bool Register(User user)
		{
			try
			{
				_context.Users.Add(user);
				_context.SaveChanges();
				return true;
			}
			catch (Exception ex)
			{
				return false;
			}
		}
		public bool ExistsEmail(string email)
		{
			try
			{
				return _context.Users.Any(u => u.Email == email.ToLower().Trim());
			}
			catch (Exception ex)
			{
				return false;
			}
		}
		public bool ExistsUsername(string username)
		{
			try
			{
				return _context.Users.Any(u => u.Username == username.ToLower().Trim());
			}
			catch (Exception ex)
			{
				return false;
			}
		}
	}
}
