namespace MMSA.DAL.Entities;

public class CourseTeacher
{
    public int CourseId { get; set; }

    public virtual Course Course { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public virtual ApplicationUser User { get; set; } = null!;

    public DateTime AddedAtUtc { get; set; }

    public string? AddedByUserId { get; set; }
}
