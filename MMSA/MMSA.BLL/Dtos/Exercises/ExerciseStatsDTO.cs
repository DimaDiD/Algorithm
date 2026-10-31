namespace MMSA.BLL.Dtos.Exercises;

public class ExerciseProgressDTO
{
    public int ExerciseId { get; set; }
    public int Attempts { get; set; }
    public int BestScore { get; set; }
    public int LastScore { get; set; }
    public int MaxScore { get; set; }
    public DateTime LastSubmittedAtUtc { get; set; }
}

public class ExerciseStatsDTO
{
    public int ExerciseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Attempts { get; set; }
    public int Students { get; set; }
    public double? AveragePercent { get; set; }
    public double? FirstAttemptAveragePercent { get; set; }
    public double? BestAveragePercent { get; set; }
    public DateTime? LastSubmittedAtUtc { get; set; }
    public List<QuestionStatsDTO> Questions { get; set; } = new();
}

public class QuestionStatsDTO
{
    public int QuestionId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public int Answered { get; set; }
    public int Correct { get; set; }
    public double? CorrectPercent { get; set; }
    public List<OptionStatsDTO> Options { get; set; } = new();
    public List<WrongAnswerStatsDTO> WrongAnswers { get; set; } = new();
}

public class OptionStatsDTO
{
    public int OptionId { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int Picks { get; set; }
}

public class WrongAnswerStatsDTO
{
    public string Text { get; set; } = string.Empty;
    public int Count { get; set; }
}
