namespace MMSA.BLL.Services.Interfaces.Ai;

public sealed record AiReviewRequest(
    string QuestionText,
    string? ReferenceAnswer,
    string? Rubric,
    int MaxPoints,
    string StudentAnswer,
    string? CodeLanguageName = null);

public sealed record AiCodeIssue(int Line, string Message);

public sealed record AiReviewVerdict(int Points, string Feedback, IReadOnlyList<AiCodeIssue> Issues);

public interface IAiReviewService
{
    bool IsEnabled { get; }

    Task<AiReviewVerdict?> ReviewAsync(AiReviewRequest request, CancellationToken cancellationToken);
}
