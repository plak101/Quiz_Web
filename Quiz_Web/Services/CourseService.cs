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
                    .Include(c => c.Category)
                    .Where(c => c.IsPublished && c.Category != null && c.Category.Slug == category)
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

        // ============= NEW METHODS FOR COURSE BUILDER =============

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
                _logger.LogError(ex, "Error retrieving categories");
                return new List<CourseCategory>();
            }
        }

        public Course? CreateCourseWithStructure(CourseBuilderViewModel model, int ownerId)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                // Step 1: Create Course
                var course = new Course
                {
                    OwnerId = ownerId,
                    CategoryId = model.CategoryId,
                    Title = model.Title,
                    Slug = model.Slug,
                    Summary = model.Summary,
                    CoverUrl = model.CoverUrl,
                    Price = model.Price ?? 0,
                    IsPublished = model.IsPublished,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Courses.Add(course);
                _context.SaveChanges();

                // Step 2: Create Chapters, Lessons, and LessonContents
                foreach (var chapterVM in model.Chapters)
                {
                    var chapter = new CourseChapter
                    {
                        CourseId = course.CourseId,
                        Title = chapterVM.Title,
                        Description = chapterVM.Description,
                        OrderIndex = chapterVM.OrderIndex
                    };

                    _context.CourseChapters.Add(chapter);
                    _context.SaveChanges();

                    foreach (var lessonVM in chapterVM.Lessons)
                    {
                        var lesson = new Lesson
                        {
                            ChapterId = chapter.ChapterId,
                            Title = lessonVM.Title,
                            Description = lessonVM.Description,
                            OrderIndex = lessonVM.OrderIndex,
                            Visibility = lessonVM.Visibility,
                            CreatedAt = DateTime.UtcNow
                        };

                        _context.Lessons.Add(lesson);
                        _context.SaveChanges();

                        foreach (var contentVM in lessonVM.Contents)
                        {
                            var content = new LessonContent
                            {
                                LessonId = lesson.LessonId,
                                ContentType = contentVM.ContentType,
                                RefId = contentVM.RefId,
                                Title = contentVM.Title,
                                Body = contentVM.Body,
                                OrderIndex = contentVM.OrderIndex,
                                CreatedAt = DateTime.UtcNow
                            };

                            _context.LessonContents.Add(content);
                        }
                        _context.SaveChanges();
                    }
                }

                transaction.Commit();
                return course;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.LogError(ex, "Error creating course with structure");
                return null;
            }
        }

        public Course? UpdateCourseStructure(int courseId, CourseBuilderViewModel model, int ownerId)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var course = _context.Courses
                    .Include(c => c.CourseChapters)
                        .ThenInclude(ch => ch.Lessons)
                            .ThenInclude(l => l.LessonContents)
                    .FirstOrDefault(c => c.CourseId == courseId && c.OwnerId == ownerId);

                if (course == null) return null;

                // Update course info
                course.CategoryId = model.CategoryId;
                course.Title = model.Title;
                course.Slug = model.Slug;
                course.Summary = model.Summary;
                course.CoverUrl = model.CoverUrl;
                course.Price = model.Price ?? 0;
                course.IsPublished = model.IsPublished;
                course.UpdatedAt = DateTime.UtcNow;

                // Remove old structure
                foreach (var chapter in course.CourseChapters.ToList())
                {
                    foreach (var lesson in chapter.Lessons.ToList())
                    {
                        _context.LessonContents.RemoveRange(lesson.LessonContents);
                        _context.Lessons.Remove(lesson);
                    }
                    _context.CourseChapters.Remove(chapter);
                }
                _context.SaveChanges();

                // Add new structure
                foreach (var chapterVM in model.Chapters)
                {
                    var chapter = new CourseChapter
                    {
                        CourseId = course.CourseId,
                        Title = chapterVM.Title,
                        Description = chapterVM.Description,
                        OrderIndex = chapterVM.OrderIndex
                    };

                    _context.CourseChapters.Add(chapter);
                    _context.SaveChanges();

                    foreach (var lessonVM in chapterVM.Lessons)
                    {
                        var lesson = new Lesson
                        {
                            ChapterId = chapter.ChapterId,
                            Title = lessonVM.Title,
                            Description = lessonVM.Description,
                            OrderIndex = lessonVM.OrderIndex,
                            Visibility = lessonVM.Visibility,
                            CreatedAt = DateTime.UtcNow
                        };

                        _context.Lessons.Add(lesson);
                        _context.SaveChanges();

                        foreach (var contentVM in lessonVM.Contents)
                        {
                            var content = new LessonContent
                            {
                                LessonId = lesson.LessonId,
                                ContentType = contentVM.ContentType,
                                RefId = contentVM.RefId,
                                Title = contentVM.Title,
                                Body = contentVM.Body,
                                OrderIndex = contentVM.OrderIndex,
                                CreatedAt = DateTime.UtcNow
                            };

                            _context.LessonContents.Add(content);
                        }
                        _context.SaveChanges();
                    }
                }

                transaction.Commit();
                return course;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.LogError(ex, "Error updating course structure");
                return null;
            }
        }

        public Course? GetCourseWithFullStructure(int courseId, int ownerId)
        {
            try
            {
                return _context.Courses
                    .Include(c => c.Category)
                    .Include(c => c.CourseChapters.OrderBy(ch => ch.OrderIndex))
                        .ThenInclude(ch => ch.Lessons.OrderBy(l => l.OrderIndex))
                            .ThenInclude(l => l.LessonContents.OrderBy(lc => lc.OrderIndex))
                    .FirstOrDefault(c => c.CourseId == courseId && c.OwnerId == ownerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving course with full structure");
                return null;
            }
        }

        public CourseBuilderViewModel? GetCourseBuilderData(int courseId, int ownerId)
        {
            try
            {
                var course = GetCourseWithFullStructure(courseId, ownerId);
                if (course == null) return null;

                var viewModel = new CourseBuilderViewModel
                {
                    Title = course.Title,
                    Slug = course.Slug,
                    Summary = course.Summary,
                    CategoryId = course.CategoryId,
                    CoverUrl = course.CoverUrl,
                    Price = course.Price,
                    IsPublished = course.IsPublished,
                    Chapters = course.CourseChapters.Select(ch => new ChapterBuilderViewModel
                    {
                        ChapterId = ch.ChapterId,
                        Title = ch.Title,
                        Description = ch.Description,
                        OrderIndex = ch.OrderIndex,
                        Lessons = ch.Lessons.Select(l => new LessonBuilderViewModel
                        {
                            LessonId = l.LessonId,
                            Title = l.Title,
                            Description = l.Description,
                            OrderIndex = l.OrderIndex,
                            Visibility = l.Visibility,
                            Contents = l.LessonContents.Select(lc => new LessonContentBuilderViewModel
                            {
                                ContentId = lc.ContentId,
                                ContentType = lc.ContentType,
                                RefId = lc.RefId,
                                Title = lc.Title,
                                Body = lc.Body,
                                OrderIndex = lc.OrderIndex
                            }).ToList()
                        }).ToList()
                    }).ToList()
                };

                return viewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting course builder data");
                return null;
            }
        }

        public bool AutosaveCourse(int? courseId, CourseAutosaveViewModel model, int ownerId)
        {
            try
            {
                if (courseId.HasValue)
                {
                    var course = _context.Courses.FirstOrDefault(c => c.CourseId == courseId && c.OwnerId == ownerId);
                    if (course != null)
                    {
                        course.Title = model.Title;
                        course.Slug = model.Slug;
                        course.Summary = model.Summary;
                        course.CategoryId = model.CategoryId;
                        course.CoverUrl = model.CoverUrl;
                        course.Price = model.Price ?? 0;
                        course.UpdatedAt = DateTime.UtcNow;
                        _context.SaveChanges();
                        return true;
                    }
                }
                else
                {
                    // Create new draft course
                    var course = new Course
                    {
                        OwnerId = ownerId,
                        CategoryId = model.CategoryId,
                        Title = model.Title,
                        Slug = model.Slug,
                        Summary = model.Summary,
                        CoverUrl = model.CoverUrl,
                        Price = model.Price ?? 0,
                        IsPublished = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Courses.Add(course);
                    _context.SaveChanges();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error autosaving course");
                return false;
            }
        }

		public bool IsSlugUnique(string slug, int? excludeCourseId)
		{
            try
            {
                return !_context.Courses.Any(c => c.Slug == slug && (!excludeCourseId.HasValue || c.CourseId != excludeCourseId));
            }catch(Exception ex)
            {
                _logger.LogError(ex, $"Error checking slug uniqueness: {slug}");
                return false;
			}
		}

        public List<Course> GetRecommendedCourses(int userId, int count = 6)
        {
            try
            {
                // Get user's interests (categories they're interested in)
                var userInterests = _context.UserInterests
                    .Where(ui => ui.UserId == userId)
                    .Select(ui => ui.CategoryId)
                    .ToList();

                // If user has interests, prioritize courses in those categories
                if (userInterests.Any())
                {
                    var recommendedCourses = _context.Courses
                        .Include(c => c.Owner)
                        .Include(c => c.Category)
                        .Where(c => c.IsPublished && 
                                   c.CategoryId.HasValue && 
                                   userInterests.Contains(c.CategoryId.Value))
                        .OrderByDescending(c => c.AverageRating)
                        .ThenByDescending(c => c.TotalReviews)
                        .ThenByDescending(c => c.CreatedAt)
                        .Take(count)
                        .ToList();

                    // If not enough courses from user interests, add popular courses
                    if (recommendedCourses.Count < count)
                    {
                        var remaining = count - recommendedCourses.Count;
                        var popularCourses = _context.Courses
                            .Include(c => c.Owner)
                            .Include(c => c.Category)
                            .Where(c => c.IsPublished && 
                                       !recommendedCourses.Select(rc => rc.CourseId).Contains(c.CourseId))
                            .OrderByDescending(c => c.AverageRating)
                            .ThenByDescending(c => c.TotalReviews)
                            .Take(remaining)
                            .ToList();

                        recommendedCourses.AddRange(popularCourses);
                    }

                    return recommendedCourses;
                }

                // If no user interests, return popular courses
                return _context.Courses
                    .Include(c => c.Owner)
                    .Include(c => c.Category)
                    .Where(c => c.IsPublished)
                    .OrderByDescending(c => c.AverageRating)
                    .ThenByDescending(c => c.TotalReviews)
                    .ThenByDescending(c => c.CreatedAt)
                    .Take(count)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recommended courses for user {UserId}", userId);
                // Fallback: return popular courses
                return _context.Courses
                    .Include(c => c.Owner)
                    .Include(c => c.Category)
                    .Where(c => c.IsPublished)
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(count)
                    .ToList();
            }
        }

        public List<Course> GetTopRatedCourses(int count = 6)
        {
            try
            {
                return _context.Courses
                    .Include(c => c.Owner)
                    .Include(c => c.Category)
                    .Where(c => c.IsPublished && c.TotalReviews > 0) // Only courses with reviews
                    .OrderByDescending(c => c.AverageRating)
                    .ThenByDescending(c => c.TotalReviews)
                    .ThenByDescending(c => c.CreatedAt)
                    .Take(count)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top rated courses");
                // Fallback: return recent published courses
                return _context.Courses
                    .Include(c => c.Owner)
                    .Include(c => c.Category)
                    .Where(c => c.IsPublished)
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(count)
                    .ToList();
            }
        }
    }
}
