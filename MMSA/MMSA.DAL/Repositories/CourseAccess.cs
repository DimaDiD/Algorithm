namespace MMSA.DAL.Repositories;

public sealed record CourseAccess(int CourseId, string? OwnerId, bool IsPublished, IReadOnlyCollection<string> TeacherIds)
{
    public bool IsOwner(string? userId) => userId is not null && OwnerId == userId;

    public bool IsTeacher(string? userId) => IsOwner(userId) || (userId is not null && TeacherIds.Contains(userId));
}
