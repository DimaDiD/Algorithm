using System.Text.RegularExpressions;
using MMSA.BLL.Dtos.Courses;
using MMSA.BLL.Services.Interfaces;
using MMSA.DAL.Entities;
using MMSA.DAL.Repositories;
using MMSA.DAL.Repositories.Interfaces;

namespace MMSA.BLL.Services.Implementation;

public sealed class CourseService : ICourseService
{
    private static readonly Regex SlugPattern = new("^[a-z0-9]+(-[a-z0-9]+)*$", RegexOptions.Compiled);

    private const int MaxLanguagesPerCourse = 10;

    private readonly ICourseRepository _courseRepository;
    private readonly IProgrammingLanguageRepository _languageRepository;
    private readonly TimeProvider _timeProvider;

    public CourseService(
        ICourseRepository courseRepository,
        IProgrammingLanguageRepository languageRepository,
        TimeProvider timeProvider)
    {
        _courseRepository = courseRepository;
        _languageRepository = languageRepository;
        _timeProvider = timeProvider;
    }

    public async Task<List<CourseDTO>> GetPublishedAsync(CurrentUser user)
    {
        var courses = await _courseRepository.GetWithPageCountAsync(query => query.Where(course => course.IsPublished));
        return courses.Select(course => ToDto(course, user)).ToList();
    }

    public async Task<List<CourseDTO>> GetMineAsync(CurrentUser user)
    {
        var courses = await _courseRepository.GetWithPageCountAsync(query =>
            user.IsAdmin ? query : query.Where(course => course.OwnerId == user.Id));
        return courses.Select(course => ToDto(course, user)).ToList();
    }

    public async Task<CourseDTO?> GetBySlugAsync(string slug, CurrentUser user)
    {
        var course = await _courseRepository.GetBySlugAsync(slug);
        if (course is null)
            return null;

        var canEdit = CanEdit(course, user);
        if (!course.IsPublished && !canEdit)
            return null;

        var dto = ToDto(course, user);
        dto.SectionCount = await _courseRepository.GetPageCountAsync(course.Id);
        return dto;
    }

    public async Task<CourseDTO> CreateAsync(CourseEditDTO dto, CurrentUser user)
    {
        var slug = NormalizeSlug(dto.Slug);
        Validate(dto, slug);

        var languages = await ResolveLanguagesAsync(dto.LanguageCodes);

        if (await _courseRepository.SlugExistsAsync(slug))
            throw new ArgumentException($"Адреса «{slug}» вже зайнята іншим курсом.");

        var course = new Course
        {
            Title = dto.Title.Trim(),
            Slug = slug,
            Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
            IsPublished = dto.IsPublished,
            OwnerId = user.Id,
            CreatedAtUtc = _timeProvider.GetUtcNow().UtcDateTime,
            Languages = languages
                .Select((language, index) => new CourseLanguage { ProgrammingLanguageId = language.Id, Order = index })
                .ToList()
        };

        await _courseRepository.InsertAsync(course, true);

        var result = ToDto(course, user);
        result.Languages = languages.Select(ProgrammingLanguageService.ToDto).ToList();
        return result;
    }

    public async Task UpdateAsync(int id, CourseEditDTO dto)
    {
        var course = await _courseRepository.GetFirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException("Курс не знайдено.");

        Validate(dto, course.Slug);
        var languages = await ResolveLanguagesAsync(dto.LanguageCodes);

        course.Title = dto.Title.Trim();
        course.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
        course.IsPublished = dto.IsPublished;

        await _courseRepository.UpdateWithLanguagesAsync(course, languages.Select(language => language.Id).ToList());
    }

    public async Task DeleteAsync(int id)
    {
        if (await _courseRepository.GetAccessForCourseAsync(id) is null)
            throw new KeyNotFoundException("Курс не знайдено.");

        await _courseRepository.DeleteCourseTreeAsync(id);
    }

    public Task<CourseAccess?> GetAccessForCourseAsync(int courseId) => _courseRepository.GetAccessForCourseAsync(courseId);
    public Task<CourseAccess?> GetAccessForPageAsync(int pageId) => _courseRepository.GetAccessForPageAsync(pageId);
    public Task<CourseAccess?> GetAccessForSubPageAsync(int subPageId) => _courseRepository.GetAccessForSubPageAsync(subPageId);
    public Task<CourseAccess?> GetAccessForTopicAsync(int pageId, int? subPageId) => _courseRepository.GetAccessForTopicAsync(pageId, subPageId);
    public Task<CourseAccess?> GetAccessForContentAsync(int pageContentId) => _courseRepository.GetAccessForContentAsync(pageContentId);
    public Task<CourseAccess?> GetAccessForExerciseAsync(int exerciseId) => _courseRepository.GetAccessForExerciseAsync(exerciseId);

    private async Task<List<ProgrammingLanguage>> ResolveLanguagesAsync(IEnumerable<string>? codes)
    {
        var normalized = (codes ?? Enumerable.Empty<string>())
            .Select(code => (code ?? string.Empty).Trim().ToLowerInvariant())
            .Where(code => code.Length > 0)
            .Distinct()
            .ToList();

        if (normalized.Count == 0)
            throw new ArgumentException("Оберіть хоча б одну мову програмування для курсу.");

        if (normalized.Count > MaxLanguagesPerCourse)
            throw new ArgumentException($"Курс може мати не більше {MaxLanguagesPerCourse} мов.");

        var found = (await _languageRepository.GetByCodesAsync(normalized)).ToDictionary(language => language.Code);
        var missing = normalized.Where(code => !found.ContainsKey(code)).ToList();
        if (missing.Count > 0)
            throw new ArgumentException($"Невідомі мови: {string.Join(", ", missing)}.");

        return normalized.Select(code => found[code]).ToList();
    }

    private static string NormalizeSlug(string? slug) => (slug ?? string.Empty).Trim().ToLowerInvariant();

    private static void Validate(CourseEditDTO dto, string slug)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new ArgumentException("Вкажіть назву курсу.");

        if (slug.Length < 3 || slug.Length > 100 || !SlugPattern.IsMatch(slug))
            throw new ArgumentException("Адреса курсу: 3–100 символів, латинські літери, цифри та дефіси (наприклад, osnovy-python).");
    }

    private static bool CanEdit(Course course, CurrentUser user) =>
        user.IsAdmin || (user.Id is not null && course.OwnerId == user.Id);

    private static CourseDTO ToDto(Course course, CurrentUser user) => new()
    {
        Id = course.Id,
        Title = course.Title,
        Slug = course.Slug,
        Description = course.Description,
        IsPublished = course.IsPublished,
        CanEdit = CanEdit(course, user),
        SectionCount = course.Pages.Count,
        Languages = course.Languages
            .Where(link => link.ProgrammingLanguage is not null)
            .OrderBy(link => link.Order)
            .Select(link => ProgrammingLanguageService.ToDto(link.ProgrammingLanguage))
            .ToList()
    };
}
