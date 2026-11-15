namespace MMSA.DAL.Repositories;

public sealed record CourseTeacherRow(
    string UserId,
    string Email,
    string? LastName,
    string? FirstName,
    string? MiddleName,
    bool IsOwner);

public sealed record TeacherCandidateRow(
    string UserId,
    string Email,
    string? LastName,
    string? FirstName,
    string? MiddleName);
