namespace MMSA.DAL.Entities;

public class CourseLanguage
{
    public int CourseId { get; set; }

    public virtual Course Course { get; set; } = null!;

    public int ProgrammingLanguageId { get; set; }

    public virtual ProgrammingLanguage ProgrammingLanguage { get; set; } = null!;

    public int Order { get; set; }
}
