using System.ComponentModel.DataAnnotations;

namespace MMSA.BLL.Dtos.Reviews;

public class CourseDashboardDTO
{
    public int CourseId { get; set; }
    public int Students { get; set; }
    public int Attempts { get; set; }
    public double? FirstAttemptAveragePercent { get; set; }
    public double? BestAveragePercent { get; set; }
    public int PendingReviews { get; set; }
    public DateTime? LastSubmittedAtUtc { get; set; }
    public List<DashboardExerciseDTO> Exercises { get; set; } = new();
}

public class DashboardExerciseDTO
{
    public int ExerciseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int PageId { get; set; }
    public string PageName { get; set; } = string.Empty;
    public int? SubPageId { get; set; }
    public string? SubPageName { get; set; }
    public int Attempts { get; set; }
    public int Students { get; set; }
    public int? MaxScore { get; set; }
    public double? FirstAttemptAverageScore { get; set; }
    public double? FirstAttemptAveragePercent { get; set; }
    public double? BestAverageScore { get; set; }
    public double? BestAveragePercent { get; set; }
    public int PendingReviews { get; set; }
}

public class ReviewItemDTO
{
    public int AnswerId { get; set; }
    public int ExerciseId { get; set; }
    public string ExerciseTitle { get; set; } = string.Empty;
    public string TopicTitle { get; set; } = string.Empty;
    public string StudentLabel { get; set; } = string.Empty;
    public DateTime SubmittedAtUtc { get; set; }
    public int QuestionId { get; set; }
    public string? QuestionText { get; set; }
    public string? QuestionType { get; set; }
    public string? CodeLanguage { get; set; }
    public string? AnswerText { get; set; }
    public string? AiFeedback { get; set; }
    public int? AiPoints { get; set; }
    public int EarnedPoints { get; set; }
    public int MaxPoints { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ReviewStatus { get; set; } = string.Empty;
    public string? TeacherComment { get; set; }
    public DateTime? ReviewedAtUtc { get; set; }
}

public class ReviewPageDTO
{
    public List<ReviewItemDTO> Items { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public class ReviewUpdateDTO
{
    [Required, MaxLength(20)]
    public string Action { get; set; } = string.Empty;

    public int? Points { get; set; }

    [MaxLength(1000)]
    public string? Comment { get; set; }
}

public static class ReviewActions
{
    public const string Confirm = "confirm";
    public const string Override = "override";
}
