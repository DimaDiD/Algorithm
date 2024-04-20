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

        [HttpDelete("DeletePageContent")]
        public async Task<IActionResult> DeletePageContent(PageContent page)
        {
            await _pageContentService.DeletePageContentAsync(page);
            return Ok();
        }
    }
}
