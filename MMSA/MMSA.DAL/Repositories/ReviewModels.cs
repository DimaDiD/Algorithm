namespace MMSA.DAL.Repositories;

public sealed record CourseExerciseInfo(
    int ExerciseId,
    string Title,
    int ExerciseOrder,
    int PageId,
    string PageName,
    int? SubPageId,
    string? SubPageName);

public sealed record SubmissionSummary(
    int ExerciseId,
    Guid ClientId,
    int Score,
    int MaxScore,
    DateTime SubmittedAtUtc);

public sealed record ReviewQuery(
    IReadOnlyCollection<int> ExerciseIds,
    int? ExerciseId,
    string? ReviewStatus,
    int Skip,
    int Take);

public sealed record ReviewRow(
    int AnswerId,
    int SubmissionId,
    int ExerciseId,
    Guid ClientId,
    DateTime SubmittedAtUtc,
    int QuestionId,
    string? QuestionText,
    string? QuestionType,
    string? CodeLanguage,
    string? AnswerText,
    string? AiFeedback,
    int? AiPoints,
    int EarnedPoints,
    int MaxPoints,
    string Status,
    string? ReviewStatus,
    string? TeacherComment,
    DateTime? ReviewedAtUtc);
