using Microsoft.EntityFrameworkCore;
using MMSA.DAL.Constants;
using MMSA.DAL.Entities;
using MMSA.DAL.Repositories.Interfaces;

namespace MMSA.DAL.Repositories.Implementations;

public class ReviewRepository : IReviewRepository
{
    private readonly AlgorithmDataContext _dataContext;

    public ReviewRepository(AlgorithmDataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public Task<List<CourseExerciseInfo>> GetCourseExercisesAsync(int courseId) =>
        (from exercise in _dataContext.Exercises
         join page in _dataContext.Pages on exercise.PageId equals page.Id
         join subPage in _dataContext.SubPages on exercise.SubPageId equals (int?)subPage.Id into subPages
         from subPage in subPages.DefaultIfEmpty()
         where page.CourseId == courseId
         orderby page.Id, subPage == null ? 0 : subPage.Id, exercise.Order
         select new CourseExerciseInfo(
             exercise.Id,
             exercise.Title,
             exercise.Order,
             page.Id,
             page.PageName,
             exercise.SubPageId,
             subPage == null ? null : subPage.Name))
        .AsNoTracking()
        .ToListAsync();

    public Task<List<SubmissionSummary>> GetSubmissionSummariesAsync(IReadOnlyCollection<int> exerciseIds) =>
        _dataContext.Submissions
            .AsNoTracking()
            .Where(submission => exerciseIds.Contains(submission.ExerciseId))
            .OrderBy(submission => submission.SubmittedAtUtc)
            .Select(submission => new SubmissionSummary(
                submission.ExerciseId,
                submission.ClientId,
                submission.Score,
                submission.MaxScore,
                submission.SubmittedAtUtc))
            .ToListAsync();

    public async Task<Dictionary<int, int>> GetPendingCountsAsync(IReadOnlyCollection<int> exerciseIds) =>
        await _dataContext.SubmissionAnswers
            .Where(answer => answer.ReviewStatus == ReviewStatuses.Pending
                && exerciseIds.Contains(answer.Submission.ExerciseId))
            .GroupBy(answer => answer.Submission.ExerciseId)
            .Select(group => new { ExerciseId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.ExerciseId, item => item.Count);

    public async Task<(List<ReviewRow> Items, int Total)> GetReviewsAsync(ReviewQuery query)
    {
        var rows =
            from answer in _dataContext.SubmissionAnswers
            join question in _dataContext.Questions on answer.QuestionId equals question.Id into questions
            from question in questions.DefaultIfEmpty()
            where answer.ReviewStatus != null && query.ExerciseIds.Contains(answer.Submission.ExerciseId)
            select new { answer, question };

        if (query.ExerciseId is { } exerciseId)
            rows = rows.Where(row => row.answer.Submission.ExerciseId == exerciseId);

        if (!string.IsNullOrEmpty(query.ReviewStatus))
            rows = query.ReviewStatus == "Reviewed"
                ? rows.Where(row => row.answer.ReviewStatus != ReviewStatuses.Pending)
                : rows.Where(row => row.answer.ReviewStatus == query.ReviewStatus);

        var total = await rows.CountAsync();

        var page = await rows
            .OrderByDescending(row => row.answer.Submission.SubmittedAtUtc)
            .ThenBy(row => row.answer.Id)
            .Skip(query.Skip)
            .Take(query.Take)
            .Select(row => new
            {
                row.answer.Id,
                row.answer.SubmissionId,
                row.answer.Submission.ExerciseId,
                row.answer.Submission.ClientId,
                row.answer.Submission.SubmittedAtUtc,
                row.answer.QuestionId,
                QuestionText = row.question == null ? null : row.question.Text,
                QuestionType = row.question == null ? (QuestionType?)null : row.question.Type,
                CodeLanguage = row.question == null ? null : row.question.CodeLanguage,
                row.answer.AnswerText,
                row.answer.AiFeedback,
                row.answer.AiPoints,
                row.answer.EarnedPoints,
                row.answer.MaxPoints,
                row.answer.Status,
                row.answer.ReviewStatus,
                row.answer.TeacherComment,
                row.answer.ReviewedAtUtc
            })
            .AsNoTracking()
            .ToListAsync();

        var items = page
            .Select(item => new ReviewRow(
                item.Id,
                item.SubmissionId,
                item.ExerciseId,
                item.ClientId,
                item.SubmittedAtUtc,
                item.QuestionId,
                item.QuestionText,
                item.QuestionType?.ToString(),
                item.CodeLanguage,
                item.AnswerText,
                item.AiFeedback,
                item.AiPoints,
                item.EarnedPoints,
                item.MaxPoints,
                item.Status,
                item.ReviewStatus,
                item.TeacherComment,
                item.ReviewedAtUtc))
            .ToList();

        return (items, total);
    }

    public Task<SubmissionAnswer?> GetAnswerWithSubmissionAsync(int answerId) =>
        _dataContext.SubmissionAnswers
            .Include(answer => answer.Submission)
                .ThenInclude(submission => submission.Answers)
            .FirstOrDefaultAsync(answer => answer.Id == answerId);

    public Task<List<SubmissionAnswer>> GetReviewedAnswersForClientAsync(Guid clientId, IReadOnlyCollection<int> exerciseIds) =>
        _dataContext.SubmissionAnswers
            .AsNoTracking()
            .Include(answer => answer.Submission)
            .Where(answer => answer.Submission.ClientId == clientId
                && exerciseIds.Contains(answer.Submission.ExerciseId)
                && answer.ReviewedAtUtc != null)
            .OrderBy(answer => answer.ReviewedAtUtc)
            .ToListAsync();

    public Task SaveAsync() => _dataContext.SaveChangesAsync();
}
