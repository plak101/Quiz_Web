using Quiz_Web.Models.Entities;

namespace Quiz_Web.Services.IServices
{
	public interface IUserService
	{
		User Login(string username, string password);
		bool Register(User user);

		bool ExistsUsername(string username);
		bool ExistsEmail(string email);

		//send mail
		User? GetUserByEmail(string email);
		bool GeneratePasswordResetToken(string email, out string token);
		bool ValidatePasswordResetToken(string token);
		bool ResetPassword(string token, string newPassword);
	}
}
