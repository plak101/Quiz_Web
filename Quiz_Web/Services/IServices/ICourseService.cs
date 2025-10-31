using Quiz_Web.Models.Entities;
using Quiz_Web.Models.ViewModels;

namespace Quiz_Web.Services.IServices
{
	public interface ICourseService
	{
		List<Course> GetAllPublishedCourses();
		Course? GetCourseById(int id);
		Course? GetCourseBySlug(string slug);
		List<Course> GetCoursesByCategory(string category);
		List<Course> SearchCourses(string keyword);
		Course? CreateCourse(CreateCourseViewModel model, int ownerId);
		bool IsSlugUnique(string slug);
		bool IsSlugUnique(string slug, int? excludeCourseId);// Overloaded method kiem tra slug loai tru khoa hoc co id truyen vao
		List<Course> GetCoursesByOwner(int ownerId);
		Course? GetOwnedCourse(int id, int ownerId);
		Course? UpdateCourse(EditCourseViewModel model, int ownerId, string? sanitizedDescription);
        bool DeleteCourse(int id, int ownerId, string? webRootPath);

		// New methods for Course Builder
		List<CourseCategory> GetAllCategories();
		Course? CreateCourseWithStructure(CourseBuilderViewModel model, int ownerId);
		Course? UpdateCourseStructure(int courseId, CourseBuilderViewModel model, int ownerId);
		Course? GetCourseWithFullStructure(int courseId, int ownerId);
		CourseBuilderViewModel? GetCourseBuilderData(int courseId, int ownerId);
		bool AutosaveCourse(int? courseId, CourseAutosaveViewModel model, int ownerId);
	}
}
