namespace MMSA.DAL.Entities;

public class Course
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string? Description { get; set; }

    public string? OwnerId { get; set; }

    public virtual ApplicationUser? Owner { get; set; }

    public bool IsPublished { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public virtual ICollection<Page> Pages { get; set; } = new List<Page>();

    public virtual ICollection<CourseLanguage> Languages { get; set; } = new List<CourseLanguage>();
}
