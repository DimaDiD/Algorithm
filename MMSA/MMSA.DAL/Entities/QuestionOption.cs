namespace MMSA.DAL.Entities;

public class QuestionOption
{
    public int Id { get; set; }

    public int QuestionId { get; set; }

    public string Text { get; set; } = null!;

    public bool IsCorrect { get; set; }

    public int Order { get; set; }

    public virtual Question Question { get; set; } = null!;
}
