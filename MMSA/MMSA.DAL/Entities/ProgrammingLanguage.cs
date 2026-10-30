namespace MMSA.DAL.Entities;

public class ProgrammingLanguage
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string HighlightLanguage { get; set; } = null!;
}
