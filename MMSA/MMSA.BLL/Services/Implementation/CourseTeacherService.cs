using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MMSA.BLL.Dtos.Courses;
using MMSA.BLL.Services.Interfaces;
using MMSA.DAL.Constants;
using MMSA.DAL.Entities;
using MMSA.DAL.Repositories;
using MMSA.DAL.Repositories.Interfaces;

namespace MMSA.BLL.Services.Implementation;

public sealed class CourseTeacherService : ICourseTeacherService
{
    private const int MinSearchLength = 2;
    private const int MaxCandidates = 10;

    private readonly ICourseTeacherRepository _teacherRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly TimeProvider _timeProvider;

    public CourseTeacherService(
        ICourseTeacherRepository teacherRepository,
        ICourseRepository courseRepository,
        UserManager<ApplicationUser> userManager,
        TimeProvider timeProvider)
    {
        _teacherRepository = teacherRepository;
        _courseRepository = courseRepository;
        _userManager = userManager;
        _timeProvider = timeProvider;
    }

    public async Task<List<CourseTeacherDTO>> GetAsync(int courseId) =>
        (await _teacherRepository.GetForCourseAsync(courseId)).Select(ToDto).ToList();

    public async Task<List<CourseTeacherDTO>> AddAsync(int courseId, string email, string? addedByUserId)
    {
        var access = await _courseRepository.GetAccessForCourseAsync(courseId)
            ?? throw new KeyNotFoundException("Курс не знайдено.");

        var user = await _userManager.FindByEmailAsync(email.Trim())
            ?? throw new ArgumentException("Користувача з таким email не знайдено. Спершу він має зареєструватися на сайті.");

        if (access.IsOwner(user.Id))
            throw new ArgumentException("Це автор курсу — він уже може редагувати курс.");

        if (!await _userManager.IsInRoleAsync(user, Roles.Teacher))
            throw new ArgumentException("Додати можна лише користувача з роллю «Викладач». Призначте роль на сторінці «Користувачі».");

        if (await _teacherRepository.ExistsAsync(courseId, user.Id))
            throw new ArgumentException("Цей викладач уже редагує курс.");

        try
        {
            await _teacherRepository.AddAsync(new CourseTeacher
            {
                CourseId = courseId,
                UserId = user.Id,
                AddedAtUtc = _timeProvider.GetUtcNow().UtcDateTime,
                AddedByUserId = addedByUserId
            });
        }
        catch (DbUpdateException)
        {
            if (!await _teacherRepository.ExistsAsync(courseId, user.Id))
                throw;

            throw new ArgumentException("Цей викладач уже редагує курс.");
        }

        return await GetAsync(courseId);
    }

    public async Task RemoveAsync(int courseId, string userId)
    {
        if (!await _teacherRepository.RemoveAsync(courseId, userId))
            throw new KeyNotFoundException("Цього викладача немає серед викладачів курсу.");
    }

    public async Task<List<CourseTeacherDTO>> SearchCandidatesAsync(int courseId, string? search)
    {
        if (string.IsNullOrWhiteSpace(search) || search.Trim().Length < MinSearchLength)
            return new List<CourseTeacherDTO>();

        var access = await _courseRepository.GetAccessForCourseAsync(courseId)
            ?? throw new KeyNotFoundException("Курс не знайдено.");

        var candidates = await _teacherRepository.SearchCandidatesAsync(courseId, access.OwnerId, Roles.Teacher, search.Trim(), MaxCandidates);
        return candidates
            .Select(candidate => new CourseTeacherDTO
            {
                UserId = candidate.UserId,
                Email = candidate.Email,
                LastName = candidate.LastName,
                FirstName = candidate.FirstName,
                MiddleName = candidate.MiddleName
            })
            .ToList();
    }

    private static CourseTeacherDTO ToDto(CourseTeacherRow row) => new()
    {
        UserId = row.UserId,
        Email = row.Email,
        LastName = row.LastName,
        FirstName = row.FirstName,
        MiddleName = row.MiddleName,
        IsOwner = row.IsOwner
    };
}
