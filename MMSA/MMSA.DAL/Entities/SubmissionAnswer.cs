namespace MMSA.DAL.Entities;

public class SubmissionAnswer
{
    public int Id { get; set; }

    public int SubmissionId { get; set; }

    public virtual Submission Submission { get; set; } = null!;

    public int QuestionId { get; set; }

    public string Status { get; set; } = null!;

    public int EarnedPoints { get; set; }

    public int MaxPoints { get; set; }

    public string? SelectedOptionIds { get; set; }

    public string? AnswerText { get; set; }
}
