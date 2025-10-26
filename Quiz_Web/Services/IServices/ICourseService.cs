using Quiz_Web.Models.Entities;

namespace Quiz_Web.Services.IServices
{
	public interface ICourseService
	{
		List<Course> GetAllPublishedCourses();
		Course? GetCourseById(int id);
		Course? GetCourseBySlug(string slug);
		List<Course> GetCoursesByCategory(string category);
		List<Course> SearchCourses(string keyword);
	}
}
