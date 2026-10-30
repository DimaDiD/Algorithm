using System.ComponentModel.DataAnnotations;

namespace MMSA.BLL.Dtos.Courses;

public class CourseDTO
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPublished { get; set; }
    public bool CanEdit { get; set; }
    public int SectionCount { get; set; }
    public List<ProgrammingLanguageDTO> Languages { get; set; } = new();
}

public class CourseEditDTO
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Slug { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    public bool IsPublished { get; set; }

    public List<string> LanguageCodes { get; set; } = new();
}

public sealed record CurrentUser(string? Id, bool IsAdmin);
