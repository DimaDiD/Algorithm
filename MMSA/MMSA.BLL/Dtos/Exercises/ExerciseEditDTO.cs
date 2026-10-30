using System.ComponentModel.DataAnnotations;

namespace MMSA.BLL.Dtos.Exercises;

public class ExerciseEditDTO
{
    public int Id { get; set; }

    public int PageId { get; set; }

    public int? SubPageId { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    public int Order { get; set; }

    [MaxLength(50)]
    public List<QuestionEditDTO> Questions { get; set; } = new();
}

public class QuestionEditDTO
{
    public int Id { get; set; }

    [Required, MaxLength(30)]
    public string Type { get; set; } = string.Empty;

    [Required, MaxLength(4000)]
    public string Text { get; set; } = string.Empty;

    [Range(1, 100)]
    public int Points { get; set; } = 1;

    [MaxLength(20)]
    public string? CodeLanguage { get; set; }

    public string? ReferenceAnswer { get; set; }

    public string? Rubric { get; set; }

    [MaxLength(10)]
    public List<QuestionOptionEditDTO> Options { get; set; } = new();
}

public class QuestionOptionEditDTO
{
    public int Id { get; set; }

    [Required, MaxLength(1000)]
    public string Text { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }
}
