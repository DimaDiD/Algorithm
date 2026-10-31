namespace MMSA.DAL.Entities;

public class Exercise
{
    public int Id { get; set; }

    public int PageId { get; set; }

    public int? SubPageId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int Order { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}
