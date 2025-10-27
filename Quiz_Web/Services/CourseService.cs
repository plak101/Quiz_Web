using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Quiz_Web.Models.EF;
using Quiz_Web.Models.Entities;
using Quiz_Web.Services.IServices;

namespace Quiz_Web.Services
{
	public class CourseService : ICourseService
	{
		private readonly LearningPlatformContext _context;
		private readonly ILogger<CourseService> _logger;

		public CourseService(LearningPlatformContext context, ILogger<CourseService> logger)
		{
			_context = context;
			_logger = logger;
		}

		public List<Course> GetAllPublishedCourses()
		{
			try
			{
				return _context.Courses.
					Include(c => c.Owner).
					Where(c => c.IsPublished).
					OrderByDescending(c => c.CreatedAt).
					ToList();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving published courses");
				return new List<Course>();
			}
		}

		public Course? GetCourseById(int id)
		{
			try
			{
				return _context.Courses
					.Include(c => c.Owner)
					.Include(c => c.CourseSections)
					.Include(c => c.CourseContents)
					.FirstOrDefault(c => c.CourseId == id && c.IsPublished);
			}catch(Exception ex)
			{
							_logger.LogError(ex, $"Error retrieving course by ID: {id}");
				return null;
			}
		}

		public Course? GetCourseBySlug(string slug)
		{
			try
			{
				return _context.Courses
					.Include(c => c.Owner)
					.Include(c => c.CourseSections)
					.Include(c => c.CourseContents)
					.FirstOrDefault(c => c.Slug == slug && c.IsPublished);
			} catch (Exception ex)
			{
				_logger.LogError($"GetCoursesByCategory error: {ex.Message}");
				return null;
			}
		}

		public List<Course> GetCoursesByCategory(string category)
		{
			try
			{
				return _context.Courses
					.Include(c => c.Owner)
					.Where(c => c.IsPublished)
					.OrderByDescending(c => c.CreatedAt)
					.ToList();
			}
			catch(Exception ex)
			{
				_logger.LogError($"GetCourseByCategory error: {ex.Message}");
				return null;
			}
		}

		public List<Course> SearchCourses(string keyword)
		{
			try
			{
				return _context.Courses
					.Include(c => c.Owner)
					.Where(c => c.IsPublished && (c.Title.Contains(keyword) || (c.Summary != null && c.Summary.Contains(keyword))))
					.OrderByDescending(c => c.CreatedAt)
					.ToList();
			}
			catch(Exception ex)
			{
				_logger.LogError($"SearchCourses error: {ex.Message}");
				return null;
			}
		}
	}
}
