using System.ComponentModel.DataAnnotations;

namespace MMSA.BLL.Dtos.Courses;

public class CourseTeacherDTO
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public bool IsOwner { get; set; }
}

public class AddCourseTeacherRequestDTO
{
    [Required, EmailAddress, MaxLength(256)]
    public string Email { get; set; } = string.Empty;
}
