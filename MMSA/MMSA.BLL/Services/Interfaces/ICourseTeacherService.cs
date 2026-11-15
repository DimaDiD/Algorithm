using MMSA.BLL.Dtos.Courses;

namespace MMSA.BLL.Services.Interfaces;

public interface ICourseTeacherService
{
    Task<List<CourseTeacherDTO>> GetAsync(int courseId);
    Task<List<CourseTeacherDTO>> AddAsync(int courseId, string email, string? addedByUserId);
    Task RemoveAsync(int courseId, string userId);
    Task<List<CourseTeacherDTO>> SearchCandidatesAsync(int courseId, string? search);
}
