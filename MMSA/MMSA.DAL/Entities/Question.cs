namespace MMSA.DAL.Entities;

public class Question
{
    public int Id { get; set; }

    public int ExerciseId { get; set; }

    public QuestionType Type { get; set; }

    public string Text { get; set; } = null!;

    public int Points { get; set; } = 1;

    public int Order { get; set; }

    public string? CodeLanguage { get; set; }

    public string? ReferenceAnswer { get; set; }

    public string? Rubric { get; set; }

    public virtual Exercise Exercise { get; set; } = null!;

    public virtual ICollection<QuestionOption> Options { get; set; } = new List<QuestionOption>();
}
