namespace MMSA.DAL.Entities;

public partial class Page
{
    public int Id { get; set; }

    public int PageId { get; set; }

    public string PageName { get; set; } = null!;

    public int? TextId { get; set; }

    public virtual ICollection<PageContent> PageContents { get; set; } = new List<PageContent>();
}
