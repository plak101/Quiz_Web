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
		List<Course> GetCoursesByOwner(int ownerId);
		Course? GetOwnedCourse(int id, int ownerId);
		Course? UpdateCourse(EditCourseViewModel model, int ownerId, string? sanitizedDescription);
        bool DeleteCourse(int id, int ownerId, string? webRootPath);
	}
}
