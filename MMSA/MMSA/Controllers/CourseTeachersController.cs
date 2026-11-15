using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MMSA.Authorization;
using MMSA.BLL.Dtos.Courses;
using MMSA.BLL.Services.Interfaces;

namespace MMSA.Controllers;

[Route("api/Courses/{courseId:int}/teachers")]
[ApiController]
[Authorize(Policy = Policies.CanEditContent)]
public class CourseTeachersController : CourseEditingControllerBase
{
    private readonly ICourseTeacherService _teacherService;
    private readonly ICourseService _courseService;

    public CourseTeachersController(ICourseTeacherService teacherService, ICourseService courseService)
    {
        _teacherService = teacherService;
        _courseService = courseService;
    }

    [HttpGet]
    public Task<IActionResult> Get(int courseId) =>
        Handle(async () =>
        {
            if (await EnsureCanEditAsync(_courseService.GetAccessForCourseAsync(courseId)) is { } denied)
                return denied;

            return Ok(await _teacherService.GetAsync(courseId));
        });

    [HttpGet("candidates")]
    public Task<IActionResult> SearchCandidates(int courseId, string? search) =>
        Handle(async () =>
        {
            if (await EnsureCanManageAsync(_courseService.GetAccessForCourseAsync(courseId)) is { } denied)
                return denied;

            return Ok(await _teacherService.SearchCandidatesAsync(courseId, search));
        });

    [HttpPost]
    public Task<IActionResult> Add(int courseId, AddCourseTeacherRequestDTO request) =>
        Handle(async () =>
        {
            if (await EnsureCanManageAsync(_courseService.GetAccessForCourseAsync(courseId)) is { } denied)
                return denied;

            return Ok(await _teacherService.AddAsync(courseId, request.Email, CurrentUser.Id));
        });

    [HttpDelete("{userId}")]
    public Task<IActionResult> Remove(int courseId, string userId) =>
        Handle(async () =>
        {
            if (await EnsureCanManageAsync(_courseService.GetAccessForCourseAsync(courseId)) is { } denied)
                return denied;

            await _teacherService.RemoveAsync(courseId, userId);
            return NoContent();
        });
}
