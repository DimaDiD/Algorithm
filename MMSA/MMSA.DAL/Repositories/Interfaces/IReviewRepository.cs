using MMSA.DAL.Entities;

namespace MMSA.DAL.Repositories.Interfaces;

public interface IReviewRepository
{
    Task<List<CourseExerciseInfo>> GetCourseExercisesAsync(int courseId);
    Task<List<SubmissionSummary>> GetSubmissionSummariesAsync(IReadOnlyCollection<int> exerciseIds);
    Task<Dictionary<int, int>> GetPendingCountsAsync(IReadOnlyCollection<int> exerciseIds);
    Task<(List<ReviewRow> Items, int Total)> GetReviewsAsync(ReviewQuery query);
    Task<SubmissionAnswer?> GetAnswerWithSubmissionAsync(int answerId);
    Task<List<SubmissionAnswer>> GetReviewedAnswersForClientAsync(Guid clientId, IReadOnlyCollection<int> exerciseIds);
    Task SaveAsync();
}
