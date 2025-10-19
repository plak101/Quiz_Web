using System.Text.RegularExpressions;

namespace Quiz_Web.Utils
{
	public class Validation
	{
		public static bool IsValidEmail(string email)
		{
			string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
			return Regex.IsMatch(email, pattern);
		}

		public static bool IsValidUsername(string username)
		{
			return username.Length >= 6;
		}
		public static bool IsValidPassword(string password)
		{
			if (password.Length < 8) return false;

			bool hasUpper = password.Any(char.IsUpper);
			bool hasLower = password.Any(char.IsLower);
			bool hasDigit = password.Any(char.IsDigit);
			bool hasSpecial = Regex.IsMatch(password, @"[\W_]");
			return hasUpper && hasLower && hasDigit && hasSpecial;
		}
	}
}
