using Microsoft.EntityFrameworkCore;
using MMSA.DAL.Entities;
using MMSA.DAL.Repositories.Interfaces;

namespace MMSA.DAL.Repositories.Implementations;

public class CourseRepository : BaseRepository<Course>, ICourseRepository
{
    public CourseRepository(AlgorithmDataContext dataContext) : base(dataContext)
    {
    }

    public Task<List<Course>> GetWithPageCountAsync(Func<IQueryable<Course>, IQueryable<Course>> filter) =>
        filter(_dataContext.Courses.AsNoTracking())
            .Include(course => course.Pages)
            .Include(course => course.Languages).ThenInclude(link => link.ProgrammingLanguage)
            .AsSplitQuery()
            .OrderBy(course => course.Id)
            .ToListAsync();

    public Task<Course?> GetBySlugAsync(string slug) =>
        _dataContext.Courses
            .AsNoTracking()
            .Include(course => course.Languages).ThenInclude(link => link.ProgrammingLanguage)
            .FirstOrDefaultAsync(course => course.Slug == slug);

    public Task<bool> SlugExistsAsync(string slug) =>
        _dataContext.Courses.AnyAsync(course => course.Slug == slug);

    public Task<int> GetPageCountAsync(int courseId) =>
        _dataContext.Pages.CountAsync(page => page.CourseId == courseId);

    public async Task<HashSet<string>> GetLanguageCodesAsync(int courseId) =>
        (await _dataContext.CourseLanguages
            .Where(link => link.CourseId == courseId)
            .Select(link => link.ProgrammingLanguage.Code)
            .ToListAsync())
        .ToHashSet();

    public async Task UpdateWithLanguagesAsync(Course course, IReadOnlyList<int> languageIds)
    {
        await using var transaction = await _dataContext.Database.BeginTransactionAsync();

        _dataContext.Courses.Update(course);
        await _dataContext.CourseLanguages.Where(link => link.CourseId == course.Id).ExecuteDeleteAsync();
        _dataContext.CourseLanguages.AddRange(languageIds.Select((languageId, index) => new CourseLanguage
        {
            CourseId = course.Id,
            ProgrammingLanguageId = languageId,
            Order = index
        }));

        await _dataContext.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public Task<CourseAccess?> GetAccessForCourseAsync(int courseId) =>
        _dataContext.Courses
            .Where(course => course.Id == courseId)
            .Select(course => new CourseAccess(course.Id, course.OwnerId))
            .FirstOrDefaultAsync();

    public Task<CourseAccess?> GetAccessForPageAsync(int pageId) =>
        _dataContext.Pages
            .Where(page => page.Id == pageId)
            .Select(page => new CourseAccess(page.CourseId, page.Course.OwnerId))
            .FirstOrDefaultAsync();

    public Task<CourseAccess?> GetAccessForSubPageAsync(int subPageId) =>
        _dataContext.SubPages
            .Where(subPage => subPage.Id == subPageId)
            .Select(subPage => new CourseAccess(subPage.Page.CourseId, subPage.Page.Course.OwnerId))
            .FirstOrDefaultAsync();

    public async Task<CourseAccess?> GetAccessForTopicAsync(int pageId, int? subPageId)
    {
        if (subPageId is null)
            return await GetAccessForPageAsync(pageId);

        return await _dataContext.SubPages
            .Where(subPage => subPage.Id == subPageId && subPage.PageId == pageId)
            .Select(subPage => new CourseAccess(subPage.Page.CourseId, subPage.Page.Course.OwnerId))
            .FirstOrDefaultAsync();
    }

    public Task<CourseAccess?> GetAccessForContentAsync(int pageContentId) =>
        (from content in _dataContext.PageContents
         join page in _dataContext.Pages on content.PageId equals (int?)page.Id
         where content.Id == pageContentId
         select new CourseAccess(page.CourseId, page.Course.OwnerId))
        .FirstOrDefaultAsync();

    public Task<CourseAccess?> GetAccessForExerciseAsync(int exerciseId) =>
        (from exercise in _dataContext.Exercises
         join page in _dataContext.Pages on exercise.PageId equals page.Id
         where exercise.Id == exerciseId
         select new CourseAccess(page.CourseId, page.Course.OwnerId))
        .FirstOrDefaultAsync();

    public async Task DeleteCourseTreeAsync(int courseId)
    {
        await using var transaction = await _dataContext.Database.BeginTransactionAsync();

        var pageIds = await _dataContext.Pages
            .Where(page => page.CourseId == courseId)
            .Select(page => page.Id)
            .ToListAsync();

        await DeletePagesAsync(pageIds);
        await _dataContext.Courses.Where(course => course.Id == courseId).ExecuteDeleteAsync();

        await transaction.CommitAsync();
    }

    public async Task DeletePageTreeAsync(int pageId)
    {
        await using var transaction = await _dataContext.Database.BeginTransactionAsync();
        await DeletePagesAsync(new List<int> { pageId });
        await transaction.CommitAsync();
    }

    public async Task DeleteSubPageTreeAsync(int subPageId)
    {
        await using var transaction = await _dataContext.Database.BeginTransactionAsync();

        await _dataContext.PageContents.Where(content => content.SubPageId == subPageId).ExecuteDeleteAsync();
        await _dataContext.Exercises.Where(exercise => exercise.SubPageId == subPageId).ExecuteDeleteAsync();
        await _dataContext.SubPages.Where(subPage => subPage.Id == subPageId).ExecuteDeleteAsync();

        await transaction.CommitAsync();
    }

    private async Task DeletePagesAsync(List<int> pageIds)
    {
        await _dataContext.PageContents.Where(content => content.PageId != null && pageIds.Contains(content.PageId.Value)).ExecuteDeleteAsync();
        await _dataContext.Exercises.Where(exercise => pageIds.Contains(exercise.PageId)).ExecuteDeleteAsync();
        await _dataContext.SubPages.Where(subPage => pageIds.Contains(subPage.PageId)).ExecuteDeleteAsync();
        await _dataContext.Pages.Where(page => pageIds.Contains(page.Id)).ExecuteDeleteAsync();
    }
}
