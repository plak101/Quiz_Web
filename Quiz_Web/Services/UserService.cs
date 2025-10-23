﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Quiz_Web.Models.EF;
using Quiz_Web.Models.Entities;
using Quiz_Web.Services.IServices;
using System.Security.Cryptography;
using System.Security.Principal;

namespace Quiz_Web.Services
{
	public class UserService : IUserService
	{
		private readonly LearningPlatformContext _context;
		private readonly ILogger<UserService> _logger;
		public UserService(LearningPlatformContext context, ILogger<UserService> logger)
		{
			_context = context;
			_logger = logger;
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

		public User? GetUserByEmail(string email)
		{
			try
			{
				return _context.Users.FirstOrDefault(u => u.Email == email.ToLower().Trim());
			}
			catch (Exception ex)
			{
				return null;
			}
		}

		public bool GeneratePasswordResetToken(string email, out string token)
		{
			try
			{
				var user = GetUserByEmail(email);
				if (user == null)
				{
					token = null;
					return false;
				}

				//random token
				token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

				user.PasswordResetToken = token;
				user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);

				_context.SaveChanges();
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError($"GeneratePasswordResetToken error: {ex.Message}");

				token = null;
				return false;
			}
		}

		public bool ValidatePasswordResetToken(string token)
		{
			try
			{
				_logger.LogInformation($"ValidatePasswordResetToken called with token: {token}");
				_logger.LogInformation($"Current UTC time: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");

				var user = _context.Users.FirstOrDefault(u => u.PasswordResetToken == token);

				if (user == null)
				{
					_logger.LogWarning("ValidatePasswordResetToken: User not found for token");
					return false;
				}

				_logger.LogInformation($"Found user: {user.Email}");
				_logger.LogInformation($"Token expiry: {user.PasswordResetTokenExpiry:yyyy-MM-dd HH:mm:ss}");

				if (user.PasswordResetTokenExpiry == null)
				{
					_logger.LogWarning("ValidatePasswordResetToken: Token expiry is null");
					return false;
				}

				bool isValid = user.PasswordResetTokenExpiry > DateTime.UtcNow;
				_logger.LogInformation($"Token is valid: {isValid}");

				return isValid;
			}
			catch (Exception ex)
			{
				_logger.LogError($"ValidatePasswordResetToken error: {ex.Message}");
				_logger.LogError($"Stack trace: {ex.StackTrace}");
				return false;
			}

		}

		public bool ResetPassword(string token, string newPassword)
		{
			try
			{
				var user = _context.Users.FirstOrDefault(u => u.PasswordResetToken == token);

				if (user == null || user.PasswordResetToken == null || user.PasswordResetTokenExpiry <= DateTime.UtcNow)
					return false;

				user.PasswordHash = newPassword;
				user.PasswordResetToken = null;
				user.PasswordResetTokenExpiry = null;

				_context.SaveChanges();
				return true;
			}catch(Exception ex)
			{
				return false;
			}
		}
	}
}
