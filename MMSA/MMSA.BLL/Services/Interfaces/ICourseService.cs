using MMSA.BLL.Dtos.Courses;
using MMSA.DAL.Repositories;

namespace MMSA.BLL.Services.Interfaces;

public interface ICourseService
{
    Task<List<CourseDTO>> GetPublishedAsync(CurrentUser user);
    Task<List<CourseDTO>> GetMineAsync(CurrentUser user);
    Task<CourseDTO?> GetBySlugAsync(string slug, CurrentUser user);
    Task<CourseDTO> CreateAsync(CourseEditDTO course, CurrentUser user);
    Task UpdateAsync(int id, CourseEditDTO course);
    Task DeleteAsync(int id);

    Task<CourseAccess?> GetAccessForCourseAsync(int courseId);
    Task<CourseAccess?> GetAccessForPageAsync(int pageId);
    Task<CourseAccess?> GetAccessForSubPageAsync(int subPageId);
    Task<CourseAccess?> GetAccessForTopicAsync(int pageId, int? subPageId);
    Task<CourseAccess?> GetAccessForContentAsync(int pageContentId);
    Task<CourseAccess?> GetAccessForExerciseAsync(int exerciseId);
}
