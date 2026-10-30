using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MMSA.Authorization;
using MMSA.BLL.Dtos;
using MMSA.BLL.Services.Interfaces;

namespace MMSA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Policies.CanEditContent)]
    public class PageContentController : CourseEditingControllerBase
    {
        private readonly IPageContentService _pageContentService;
        private readonly ICourseService _courseService;

        public PageContentController(IPageContentService pageContentService, ICourseService courseService)
        {
            _pageContentService = pageContentService;
            _courseService = courseService;
        }

        [AllowAnonymous]
        [HttpGet("GetPageContentByMenuStatus")]
        public async Task<IActionResult> GetPageContentByMenuStatus(int pageId, int? subPageId, string codeStatus)
        {
            return Ok(await _pageContentService.GetPageContentBySettingStatusAsync(pageId, subPageId, codeStatus));
        }

        [HttpPost("CreatePageContent")]
        public Task<IActionResult> CreatePageContent(PageContentDTO page) =>
            Handle(async () =>
            {
                if (await EnsureCanEditAsync(_courseService.GetAccessForTopicAsync(page.PageId, page.SubPageId)) is { } denied)
                    return denied;

                await _pageContentService.CreatePageContentAsync(page);
                return Ok();
            });

        [HttpPut("UpdatePageContent")]
        public Task<IActionResult> UpdatePageContent(PageContentDTO page) =>
            Handle(async () =>
            {
                if (await EnsureCanEditAsync(_courseService.GetAccessForContentAsync(page.Id)) is { } denied)
                    return denied;

                await _pageContentService.UpdatePageContentAsync(page);
                return Ok();
            });

        [HttpGet("GetProgrammingTopics")]
        public async Task<IActionResult> GetProgrammingTopics()
        {
            return Ok(await _pageContentService.GetProgrammingTopicsAsync());
        }

        [HttpDelete("DeletePageContent")]
        public Task<IActionResult> DeletePageContent(int pageContentId) =>
            Handle(async () =>
            {
                if (await EnsureCanEditAsync(_courseService.GetAccessForContentAsync(pageContentId)) is { } denied)
                    return denied;

                await _pageContentService.DeletePageContentAsync(pageContentId);
                return Ok();
            });
    }
}
