using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MMSA.Authorization;
using MMSA.BLL.Services.Interfaces;

namespace MMSA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Policies.CanEditContent)]
    public class SubPageController : CourseEditingControllerBase
    {
        private readonly ISubPageService _subPageService;
        private readonly ICourseService _courseService;

        public SubPageController(ISubPageService subPageService, ICourseService courseService)
        {
            _subPageService = subPageService;
            _courseService = courseService;
        }

        [HttpPost("CreateSubPage")]
        public Task<IActionResult> CreateSubPage(int pageId, string subPageName) =>
            Handle(async () =>
            {
                if (await EnsureCanEditAsync(_courseService.GetAccessForPageAsync(pageId)) is { } denied)
                    return denied;

                await _subPageService.CreateSubPageAsync(pageId, subPageName);
                return Ok();
            });

        [HttpPut("RenameSubPage")]
        public Task<IActionResult> RenameSubPage(int subPageId, string newName) =>
            Handle(async () =>
            {
                if (await EnsureCanEditAsync(_courseService.GetAccessForSubPageAsync(subPageId)) is { } denied)
                    return denied;

                await _subPageService.RenameSubPageAsync(subPageId, newName);
                return Ok();
            });

        [HttpDelete("DeleteSubPage")]
        public Task<IActionResult> DeleteSubPage(int subPageId) =>
            Handle(async () =>
            {
                if (await EnsureCanEditAsync(_courseService.GetAccessForSubPageAsync(subPageId)) is { } denied)
                    return denied;

                await _subPageService.DeleteSubPageAsync(subPageId);
                return Ok();
            });
    }
}
