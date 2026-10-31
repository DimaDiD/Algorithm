namespace MMSA.DAL.Entities;

public class Submission
{
    public int Id { get; set; }

    public int ExerciseId { get; set; }

    public virtual Exercise Exercise { get; set; } = null!;

    public Guid ClientId { get; set; }

    public int Score { get; set; }

    public int MaxScore { get; set; }

    public DateTime SubmittedAtUtc { get; set; }

    public virtual ICollection<SubmissionAnswer> Answers { get; set; } = new List<SubmissionAnswer>();
}
