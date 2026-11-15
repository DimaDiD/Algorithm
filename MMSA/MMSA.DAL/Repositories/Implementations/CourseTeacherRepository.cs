using Microsoft.EntityFrameworkCore;
using MMSA.DAL.Entities;
using MMSA.DAL.Repositories.Interfaces;

namespace MMSA.DAL.Repositories.Implementations;

public class CourseTeacherRepository : ICourseTeacherRepository
{
    private readonly AlgorithmDataContext _dataContext;

    public CourseTeacherRepository(AlgorithmDataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public async Task<List<CourseTeacherRow>> GetForCourseAsync(int courseId)
    {
        var owner = await _dataContext.Courses
            .AsNoTracking()
            .Where(course => course.Id == courseId && course.Owner != null)
            .Select(course => new CourseTeacherRow(
                course.Owner!.Id,
                course.Owner.Email ?? string.Empty,
                course.Owner.LastName,
                course.Owner.FirstName,
                course.Owner.MiddleName,
                true))
            .FirstOrDefaultAsync();

        var teachers = await _dataContext.CourseTeachers
            .AsNoTracking()
            .Where(teacher => teacher.CourseId == courseId)
            .OrderBy(teacher => teacher.User.LastName == null)
            .ThenBy(teacher => teacher.User.LastName)
            .ThenBy(teacher => teacher.User.FirstName)
            .ThenBy(teacher => teacher.User.Email)
            .Select(teacher => new CourseTeacherRow(
                teacher.UserId,
                teacher.User.Email ?? string.Empty,
                teacher.User.LastName,
                teacher.User.FirstName,
                teacher.User.MiddleName,
                false))
            .ToListAsync();

        if (owner is not null)
            teachers.Insert(0, owner);

        return teachers;
    }

    public Task<bool> ExistsAsync(int courseId, string userId) =>
        _dataContext.CourseTeachers.AnyAsync(teacher => teacher.CourseId == courseId && teacher.UserId == userId);

    public async Task AddAsync(CourseTeacher teacher)
    {
        _dataContext.CourseTeachers.Add(teacher);
        await _dataContext.SaveChangesAsync();
    }

    public async Task<bool> RemoveAsync(int courseId, string userId) =>
        await _dataContext.CourseTeachers
            .Where(teacher => teacher.CourseId == courseId && teacher.UserId == userId)
            .ExecuteDeleteAsync() > 0;

    public Task<List<TeacherCandidateRow>> SearchCandidatesAsync(int courseId, string? ownerId, string roleName, string search, int take)
    {
        var roleIds = _dataContext.Roles.Where(role => role.Name == roleName).Select(role => role.Id);

        return _dataContext.Users
            .AsNoTracking()
            .Where(user => _dataContext.UserRoles.Any(userRole => userRole.UserId == user.Id && roleIds.Contains(userRole.RoleId)))
            .Where(user => user.Id != ownerId)
            .Where(user => !_dataContext.CourseTeachers.Any(teacher => teacher.CourseId == courseId && teacher.UserId == user.Id))
            .MatchingSearch(search)
            .OrderByName()
            .Take(take)
            .Select(user => new TeacherCandidateRow(user.Id, user.Email ?? string.Empty, user.LastName, user.FirstName, user.MiddleName))
            .ToListAsync();
    }
}
