using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MMSA.Authorization;
using MMSA.BLL.Services.Interfaces;

namespace MMSA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Policies.CanEditContent)]
    public class PageController : CourseEditingControllerBase
    {
        private readonly IPageService _pageService;
        private readonly ICourseService _courseService;

        public PageController(IPageService pageService, ICourseService courseService)
        {
            _pageService = pageService;
            _courseService = courseService;
        }

        [AllowAnonymous]
        [HttpGet("GetMenuItems")]
        public async Task<IActionResult> GetMenuItems(int courseId)
        {
            return Ok(await _pageService.GetMenuItemsAsync(courseId));
        }

        [HttpPost("CreatePage")]
        public Task<IActionResult> CreatePage(int courseId, string pageName) =>
            Handle(async () =>
            {
                if (await EnsureCanEditAsync(_courseService.GetAccessForCourseAsync(courseId)) is { } denied)
                    return denied;

                await _pageService.CreatePageAsync(courseId, pageName);
                return Ok();
            });

        [HttpPut("RenamePage")]
        public Task<IActionResult> RenamePage(int pageId, string newName) =>
            Handle(async () =>
            {
                if (await EnsureCanEditAsync(_courseService.GetAccessForPageAsync(pageId)) is { } denied)
                    return denied;

                await _pageService.RenamePageAsync(pageId, newName);
                return Ok();
            });

        [HttpDelete("DeletePage")]
        public Task<IActionResult> DeletePage(int pageId) =>
            Handle(async () =>
            {
                if (await EnsureCanEditAsync(_courseService.GetAccessForPageAsync(pageId)) is { } denied)
                    return denied;

                await _pageService.DeletePageAsync(pageId);
                return Ok();
            });
    }
}
