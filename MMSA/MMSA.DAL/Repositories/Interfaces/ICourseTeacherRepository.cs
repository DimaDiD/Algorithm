using MMSA.DAL.Entities;

namespace MMSA.DAL.Repositories.Interfaces;

public interface ICourseTeacherRepository
{
    Task<List<CourseTeacherRow>> GetForCourseAsync(int courseId);
    Task<bool> ExistsAsync(int courseId, string userId);
    Task AddAsync(CourseTeacher teacher);
    Task<bool> RemoveAsync(int courseId, string userId);
    Task<List<TeacherCandidateRow>> SearchCandidatesAsync(int courseId, string? ownerId, string roleName, string search, int take);
}
