using MMSA.DAL.Entities;

namespace MMSA.DAL.Repositories.Interfaces;

public interface ICourseRepository : IBaseRepository<Course>
{
    Task<List<Course>> GetWithPageCountAsync(Func<IQueryable<Course>, IQueryable<Course>> filter);
    Task<Course?> GetBySlugAsync(string slug);
    Task<bool> SlugExistsAsync(string slug);
    Task<int> GetPageCountAsync(int courseId);
    Task<HashSet<string>> GetLanguageCodesAsync(int courseId);
    Task UpdateWithLanguagesAsync(Course course, IReadOnlyList<int> languageIds);

    Task<CourseAccess?> GetAccessForCourseAsync(int courseId);
    Task<CourseAccess?> GetAccessForPageAsync(int pageId);
    Task<CourseAccess?> GetAccessForSubPageAsync(int subPageId);
    Task<CourseAccess?> GetAccessForTopicAsync(int pageId, int? subPageId);
    Task<CourseAccess?> GetAccessForContentAsync(int pageContentId);
    Task<CourseAccess?> GetAccessForExerciseAsync(int exerciseId);

    Task DeleteCourseTreeAsync(int courseId);
    Task DeletePageTreeAsync(int pageId);
    Task DeleteSubPageTreeAsync(int subPageId);
}
