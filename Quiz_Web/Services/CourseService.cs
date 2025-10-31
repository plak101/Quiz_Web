using Microsoft.EntityFrameworkCore;
using Quiz_Web.Models.EF;
using Quiz_Web.Models.Entities;
using Quiz_Web.Models.ViewModels;
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
                return _context.Courses
                    .Include(c => c.Owner)
                    .Where(c => c.IsPublished)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToList();
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
                    //.Include(c => c.CourseSections)
                    //.Include(c => c.CourseContents)
                    .FirstOrDefault(c => c.CourseId == id && c.IsPublished);
            }
            catch (Exception ex)
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
                    .FirstOrDefault(c => c.Slug == slug && c.IsPublished);
            }
            catch (Exception ex)
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
            catch (Exception ex)
            {
                _logger.LogError($"GetCourseByCategory error: {ex.Message}");
                return new List<Course>();
            }
        }

        public List<Course> SearchCourses(string keyword)
        {
            try
            {
                return _context.Courses
                    .Include(c => c.Owner)
                    .Where(c => c.IsPublished &&
                        (c.Title.Contains(keyword) || (c.Summary != null && c.Summary.Contains(keyword))))
                    .OrderByDescending(c => c.CreatedAt)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"SearchCourses error: {ex.Message}");
                return new List<Course>();
            }
        }

        public Course? CreateCourse(CreateCourseViewModel model, int ownerId)
        {
            try
            {
                var course = new Course
                {
                    OwnerId = ownerId,
                    Title = model.Title,
                    Slug = model.Slug,
                    Summary = model.Description,
                    CoverUrl = model.CoverUrl,
                    Price = model.Price ?? 0,
                    //Currency = model.Currency,
                    IsPublished = model.IsPublished,
                    CreatedAt = DateTime.Now
                };

                _context.Courses.Add(course);
                _context.SaveChanges();

                return course;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating course");
                return null;
            }
        }

        public bool IsSlugUnique(string slug)
        {
            try
            {
                return !_context.Courses.Any(c => c.Slug == slug);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking slug uniqueness: {slug}");
                return false;
            }
        }

        // Added
        public List<Course> GetCoursesByOwner(int ownerId)
        {
            try
            {
                return _context.Courses
                    .Include(c => c.Owner)
                    .Where(c => c.OwnerId == ownerId)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving owner courses");
                return new List<Course>();
            }
        }

        public Course? GetOwnedCourse(int id, int ownerId)
        {
            try
            {
                return _context.Courses
                    .Include(c => c.Owner)
                    .FirstOrDefault(c => c.CourseId == id && c.OwnerId == ownerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving owned course");
                return null;
            }
        }

        public Course? UpdateCourse(EditCourseViewModel model, int ownerId, string? sanitizedDescription)
        {
            try
            {
                var course = _context.Courses.FirstOrDefault(c => c.CourseId == model.CourseId && c.OwnerId == ownerId);
                if (course == null) return null;

                course.Title = model.Title;
                course.Slug = model.Slug;
                course.Summary = sanitizedDescription ?? model.Description;
                course.Price = model.Price ?? 0;
                //course.Currency = model.Currency;
                course.IsPublished = model.IsPublished;
                course.CoverUrl = model.CoverUrl;
                course.UpdatedAt = DateTime.UtcNow;

                _context.SaveChanges();
                return course;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating course");
                return null;
            }
        }

        public bool DeleteCourse(int id, int ownerId, string? webRootPath)
        {
            try
            {
                var course = _context.Courses.FirstOrDefault(c => c.CourseId == id && c.OwnerId == ownerId);
                if (course == null) return false;

                // Try delete physical cover file if stored locally
                if (!string.IsNullOrWhiteSpace(course.CoverUrl) &&
                    webRootPath != null &&
                    course.CoverUrl.StartsWith("/"))
                {
                    var relative = course.CoverUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString());
                    var physicalPath = Path.Combine(webRootPath, relative);
                    if (System.IO.File.Exists(physicalPath))
                        System.IO.File.Delete(physicalPath);
                }

                _context.Courses.Remove(course);
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting course {CourseId}", id);
                return false;
            }
        }

        public List<CourseCategory> GetAllCategories()
        {
            try
            {
                return _context.CourseCategories
                    .OrderBy(c => c.DisplayOrder)
                    .ThenBy(c => c.Name)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving course categories");
                return new List<CourseCategory>();
            }
        }
    }
}
