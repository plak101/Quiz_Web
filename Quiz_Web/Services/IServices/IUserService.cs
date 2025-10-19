using Quiz_Web.Models.Entities;

namespace Quiz_Web.Services.IServices
{
	public interface IUserService
	{
		User Login(string username, string password);
		bool Register(User user);

		bool ExistsUsername(string username);
		bool ExistsEmail(string email);
	}
}
