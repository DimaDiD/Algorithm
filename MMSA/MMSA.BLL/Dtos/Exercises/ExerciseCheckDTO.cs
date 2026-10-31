using System.ComponentModel.DataAnnotations;

namespace MMSA.BLL.Dtos.Exercises;

public class ExerciseCheckRequestDTO
{
    public Guid? ClientId { get; set; }

    [MaxLength(50)]
    public List<QuestionAnswerDTO> Answers { get; set; } = new();
}

public class QuestionAnswerDTO
{
    public int QuestionId { get; set; }

    [MaxLength(10)]
    public List<int> SelectedOptionIds { get; set; } = new();

    [MaxLength(10000)]
    public string? Text { get; set; }
}

public class ExerciseCheckResultDTO
{
    public int Score { get; set; }
    public int MaxScore { get; set; }
    public List<QuestionResultDTO> Questions { get; set; } = new();
}

public class QuestionResultDTO
{
    public int QuestionId { get; set; }
    public string Status { get; set; } = string.Empty;
    public int EarnedPoints { get; set; }
    public int MaxPoints { get; set; }
    public List<int> CorrectOptionIds { get; set; } = new();
    public List<string> AcceptedAnswers { get; set; } = new();
    public string? Feedback { get; set; }
    public bool IsAiReviewed { get; set; }
}

public sealed record ExerciseCheckContext(bool RecordSubmission, string ClientKey);

public class AiStatusDTO
{
    public bool Enabled { get; set; }
    public int MaxAnswerLength { get; set; }
}

public static class QuestionResultStatus
{
    public const string Correct = "Correct";
    public const string Incorrect = "Incorrect";
    public const string PartiallyCorrect = "PartiallyCorrect";
    public const string NotChecked = "NotChecked";
}
