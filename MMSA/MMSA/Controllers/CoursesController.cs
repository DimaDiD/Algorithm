using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MMSA.Authorization;
using MMSA.BLL.Dtos.Courses;
using MMSA.BLL.Services.Interfaces;

namespace MMSA.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Policy = Policies.CanEditContent)]
public class CoursesController : CourseEditingControllerBase
{
    private readonly ICourseService _courseService;

    public CoursesController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetPublished()
    {
        return Ok(await _courseService.GetPublishedAsync(CurrentUser));
    }

    [HttpGet("mine")]
    public async Task<IActionResult> GetMine()
    {
        return Ok(await _courseService.GetMineAsync(CurrentUser));
    }

    [AllowAnonymous]
    [HttpGet("by-slug/{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var course = await _courseService.GetBySlugAsync(slug, CurrentUser);
        return course is null ? NotFound() : Ok(course);
    }

    [HttpPost]
    public Task<IActionResult> Create(CourseEditDTO course) =>
        Handle(async () => Ok(await _courseService.CreateAsync(course, CurrentUser)));

    [HttpPut("{id:int}")]
    public Task<IActionResult> Update(int id, CourseEditDTO course) =>
        Handle(async () =>
        {
            if (await EnsureCanEditAsync(_courseService.GetAccessForCourseAsync(id)) is { } denied)
                return denied;

            await _courseService.UpdateAsync(id, course);
            return NoContent();
        });

    [HttpDelete("{id:int}")]
    public Task<IActionResult> Delete(int id) =>
        Handle(async () =>
        {
            if (await EnsureCanEditAsync(_courseService.GetAccessForCourseAsync(id)) is { } denied)
                return denied;

            await _courseService.DeleteAsync(id);
            return NoContent();
        });
}
