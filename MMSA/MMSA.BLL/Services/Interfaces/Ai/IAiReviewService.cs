namespace MMSA.BLL.Services.Interfaces.Ai;

public sealed record AiReviewRequest(
    string QuestionText,
    string? ReferenceAnswer,
    string? Rubric,
    int MaxPoints,
    string StudentAnswer);

public sealed record AiReviewVerdict(int Points, string Feedback);

public interface IAiReviewService
{
    bool IsEnabled { get; }

    Task<AiReviewVerdict?> ReviewAsync(AiReviewRequest request, CancellationToken cancellationToken);
}
