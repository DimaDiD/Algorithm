using Microsoft.AspNetCore.Mvc;
using MMSA.BLL.Services.Interfaces;
using MMSA.DAL.Entities;

namespace MMSA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PageContentController : ControllerBase
    {
        private readonly IPageContentService _pageContentService;
        public PageContentController(IPageContentService pageContentService)
        {
            _pageContentService = pageContentService;
        }

        [HttpGet("GetPageContentByMenuStatus")]
        public async Task<IActionResult> GetPageContentByMenuStatus(int pageId, int? subPageId)
        {
            var pageContent = await _pageContentService.GetPageContentBySettingStatusAsync(pageId, subPageId);
            return Ok(pageContent);
        }

        [HttpPost("CreatePageContent")]
        public async Task<IActionResult> CreatePageContent(PageContent page)
        {
            await _pageContentService.CreatePageContentAsync(page);
            return Ok();
        }

        [HttpPut("UpdatePageContent")]
        public async Task<IActionResult> UpdatePageContent(PageContent page)
        {
            await _pageContentService.UpdatePageContentAsync(page);
            return Ok();
        }

        [HttpDelete("DeletePageContent")]
        public async Task<IActionResult> DeletePageContent(int pageContentId)
        {
            await _pageContentService.DeletePageContentAsync(pageContentId);
            return Ok();
        }
    }
}
