using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MMSA.Authorization;
using MMSA.BLL.Dtos.Reviews;
using MMSA.BLL.Services.Interfaces;

namespace MMSA.Controllers;

[ApiController]
[Authorize(Policy = Policies.CanEditContent)]
public class CourseDashboardController : CourseEditingControllerBase
{
    private readonly ICourseDashboardService _dashboardService;
    private readonly ICourseService _courseService;

    public CourseDashboardController(ICourseDashboardService dashboardService, ICourseService courseService)
    {
        _dashboardService = dashboardService;
        _courseService = courseService;
    }

    [HttpGet("api/Courses/{courseId:int}/dashboard")]
    public Task<IActionResult> GetDashboard(int courseId) =>
        Handle(async () =>
        {
            if (await EnsureCanEditAsync(_courseService.GetAccessForCourseAsync(courseId)) is { } denied)
                return denied;

            return Ok(await _dashboardService.GetDashboardAsync(courseId));
        });

    [HttpGet("api/Courses/{courseId:int}/reviews")]
    public Task<IActionResult> GetReviews(int courseId, int? exerciseId, string? status, int page = 1) =>
        Handle(async () =>
        {
            if (await EnsureCanEditAsync(_courseService.GetAccessForCourseAsync(courseId)) is { } denied)
                return denied;

            return Ok(await _dashboardService.GetReviewsAsync(courseId, exerciseId, status, page));
        });

    [HttpPut("api/Reviews/{answerId:int}")]
    public Task<IActionResult> Review(int answerId, ReviewUpdateDTO review) =>
        Handle(async () =>
        {
            if (await EnsureCanEditAsync(_courseService.GetAccessForSubmissionAnswerAsync(answerId)) is { } denied)
                return denied;

            await _dashboardService.ReviewAsync(answerId, review, CurrentUser.Id);
            return NoContent();
        });
}
