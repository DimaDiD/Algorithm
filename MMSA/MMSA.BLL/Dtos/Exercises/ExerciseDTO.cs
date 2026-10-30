namespace MMSA.BLL.Dtos.Exercises;

public class ExerciseDTO
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Order { get; set; }
    public List<QuestionDTO> Questions { get; set; } = new();
}

public class QuestionDTO
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public int Points { get; set; }
    public int Order { get; set; }
    public string? CodeLanguage { get; set; }
    public List<QuestionOptionDTO> Options { get; set; } = new();
}

public class QuestionOptionDTO
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
}
