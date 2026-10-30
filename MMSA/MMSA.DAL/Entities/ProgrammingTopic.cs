namespace MMSA.DAL.Entities;

public class ProgrammingTopic
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<PageContent> PageContents { get; set; } = new List<PageContent>();
}
