using MMSA.BLL.Dtos.Exercises;
using MMSA.BLL.Dtos.Reviews;
using MMSA.BLL.Services.Interfaces;
using MMSA.DAL.Constants;
using MMSA.DAL.Repositories;
using MMSA.DAL.Repositories.Interfaces;

namespace MMSA.BLL.Services.Implementation;

public sealed class CourseDashboardService : ICourseDashboardService
{
    private const int PageSize = 20;
    private const int CommentMaxLength = 1000;

    private readonly IReviewRepository _reviewRepository;
    private readonly TimeProvider _timeProvider;

    public CourseDashboardService(IReviewRepository reviewRepository, TimeProvider timeProvider)
    {
        _reviewRepository = reviewRepository;
        _timeProvider = timeProvider;
    }

    public async Task<CourseDashboardDTO> GetDashboardAsync(int courseId)
    {
        var exercises = await _reviewRepository.GetCourseExercisesAsync(courseId);
        var exerciseIds = exercises.Select(e => e.ExerciseId).ToList();
        var submissions = await _reviewRepository.GetSubmissionSummariesAsync(exerciseIds);
        var pending = await _reviewRepository.GetPendingCountsAsync(exerciseIds);
        var byExercise = submissions.ToLookup(s => s.ExerciseId);

        return new CourseDashboardDTO
        {
            CourseId = courseId,
            Students = submissions.Select(s => s.ClientId).Distinct().Count(),
            Attempts = submissions.Count,
            FirstAttemptAveragePercent = AveragePercent(FirstAttempts(submissions)),
            BestAveragePercent = AveragePercent(BestAttempts(submissions)),
            PendingReviews = pending.Values.Sum(),
            LastSubmittedAtUtc = submissions.Count == 0 ? null : submissions.Max(s => s.SubmittedAtUtc),
            Exercises = exercises
                .Select(exercise =>
                {
                    var items = byExercise[exercise.ExerciseId].ToList();
                    var firstAttempts = FirstAttempts(items).ToList();
                    var bestAttempts = BestAttempts(items).ToList();
                    return new DashboardExerciseDTO
                    {
                        ExerciseId = exercise.ExerciseId,
                        Title = exercise.Title,
                        PageId = exercise.PageId,
                        PageName = exercise.PageName,
                        SubPageId = exercise.SubPageId,
                        SubPageName = exercise.SubPageName,
                        Attempts = items.Count,
                        Students = items.Select(s => s.ClientId).Distinct().Count(),
                        MaxScore = items.Count == 0 ? null : items.MaxBy(s => s.SubmittedAtUtc)!.MaxScore,
                        FirstAttemptAverageScore = AverageScore(firstAttempts),
                        FirstAttemptAveragePercent = AveragePercent(firstAttempts),
                        BestAverageScore = AverageScore(bestAttempts),
                        BestAveragePercent = AveragePercent(bestAttempts),
                        PendingReviews = pending.GetValueOrDefault(exercise.ExerciseId)
                    };
                })
                .ToList()
        };
    }

    public async Task<ReviewPageDTO> GetReviewsAsync(int courseId, int? exerciseId, string? status, int page)
    {
        page = Math.Max(page, 1);
        var exercises = (await _reviewRepository.GetCourseExercisesAsync(courseId)).ToDictionary(e => e.ExerciseId);
        var result = new ReviewPageDTO { Page = page, PageSize = PageSize };

        if (exerciseId is { } id && !exercises.ContainsKey(id))
            return result;

        var normalizedStatus = status switch
        {
            null or "" or "all" => null,
            "Reviewed" => "Reviewed",
            _ when ReviewStatuses.All.Contains(status) => status,
            _ => throw new ArgumentException("Невідомий фільтр статусу.")
        };

        var (rows, total) = await _reviewRepository.GetReviewsAsync(new ReviewQuery(
            exercises.Keys.ToList(),
            exerciseId,
            normalizedStatus,
            (page - 1) * PageSize,
            PageSize));

        result.Total = total;
        result.Items = rows.Select(row => ToDto(row, exercises[row.ExerciseId])).ToList();
        return result;
    }

    public async Task ReviewAsync(int answerId, ReviewUpdateDTO review, string? reviewerId)
    {
        var answer = await _reviewRepository.GetAnswerWithSubmissionAsync(answerId)
            ?? throw new KeyNotFoundException("Відповідь не знайдено.");

        if (answer.ReviewStatus is null)
            throw new ArgumentException("Ця відповідь не потребує перегляду викладача.");

        var comment = string.IsNullOrWhiteSpace(review.Comment) ? null : review.Comment.Trim();
        if (comment?.Length > CommentMaxLength)
            throw new ArgumentException($"Коментар — не довше {CommentMaxLength} символів.");

        switch (review.Action)
        {
            case ReviewActions.Confirm:
                if (answer.AiPoints is null)
                    throw new ArgumentException("AI не оцінив цю відповідь — вкажіть бали самостійно.");
                answer.EarnedPoints = answer.AiPoints.Value;
                answer.ReviewStatus = ReviewStatuses.Confirmed;
                break;

            case ReviewActions.Override:
                if (review.Points is not { } points || points < 0 || points > answer.MaxPoints)
                    throw new ArgumentException($"Бали мають бути від 0 до {answer.MaxPoints}.");
                answer.EarnedPoints = points;
                answer.ReviewStatus = ReviewStatuses.Overridden;
                break;

            default:
                throw new ArgumentException("Невідома дія перегляду.");
        }

        answer.Status = QuestionResultStatus.FromPoints(answer.EarnedPoints, answer.MaxPoints);
        answer.TeacherComment = comment;
        answer.ReviewedAtUtc = _timeProvider.GetUtcNow().UtcDateTime;
        answer.ReviewedByUserId = reviewerId;

        var counted = answer.Submission.Answers.Where(a => a.Status != QuestionResultStatus.NotChecked).ToList();
        answer.Submission.Score = counted.Sum(a => a.EarnedPoints);
        answer.Submission.MaxScore = counted.Sum(a => a.MaxPoints);

        await _reviewRepository.SaveAsync();
    }

    private static ReviewItemDTO ToDto(ReviewRow row, CourseExerciseInfo exercise) => new()
    {
        AnswerId = row.AnswerId,
        ExerciseId = row.ExerciseId,
        ExerciseTitle = exercise.Title,
        TopicTitle = exercise.SubPageName is null ? exercise.PageName : $"{exercise.PageName} / {exercise.SubPageName}",
        StudentLabel = StudentLabel(row.ClientId),
        SubmittedAtUtc = row.SubmittedAtUtc,
        QuestionId = row.QuestionId,
        QuestionText = row.QuestionText,
        QuestionType = row.QuestionType,
        CodeLanguage = row.CodeLanguage,
        AnswerText = row.AnswerText,
        AiFeedback = row.AiFeedback,
        AiPoints = row.AiPoints,
        EarnedPoints = row.EarnedPoints,
        MaxPoints = row.MaxPoints,
        Status = row.Status,
        ReviewStatus = row.ReviewStatus ?? ReviewStatuses.Pending,
        TeacherComment = row.TeacherComment,
        ReviewedAtUtc = row.ReviewedAtUtc
    };

    private static string StudentLabel(Guid clientId) => $"Учень #{clientId.ToString("N")[..6]}";

    private static IEnumerable<SubmissionSummary> FirstAttempts(IEnumerable<SubmissionSummary> submissions) =>
        submissions
            .GroupBy(s => (s.ExerciseId, s.ClientId))
            .Select(group => group.MinBy(s => s.SubmittedAtUtc)!);

    private static IEnumerable<SubmissionSummary> BestAttempts(IEnumerable<SubmissionSummary> submissions) =>
        submissions
            .GroupBy(s => (s.ExerciseId, s.ClientId))
            .Select(group => group.MaxBy(Percent)!);

    private static double Percent(SubmissionSummary submission) =>
        submission.MaxScore == 0 ? 0 : 100.0 * submission.Score / submission.MaxScore;

    private static double? AverageScore(IReadOnlyCollection<SubmissionSummary> submissions) =>
        submissions.Count == 0 ? null : Math.Round(submissions.Average(s => s.Score), 1);

    private static double? AveragePercent(IEnumerable<SubmissionSummary> submissions)
    {
        var percents = submissions
            .Where(s => s.MaxScore > 0)
            .Select(s => 100.0 * s.Score / s.MaxScore)
            .ToList();

        return percents.Count == 0 ? null : Math.Round(percents.Average(), 1);
    }
}
