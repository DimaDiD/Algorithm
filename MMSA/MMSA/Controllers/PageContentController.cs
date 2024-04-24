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
        public async Task<IActionResult> GetPageContentByMenuStatus(int pageId, int? subPageId, string codeStatus)
        {
            return Ok(await _pageContentService.GetPageContentBySettingStatusAsync(pageId, subPageId, codeStatus));
        }

        [HttpPost("CreatePageContent")]
        public async Task<IActionResult> CreatePageContent(PageContent page)
        {
            try
            {
                await _pageContentService.CreatePageContentAsync(page);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("UpdatePageContent")]
        public async Task<IActionResult> UpdatePageContent(PageContent page)
        {
            try
            {
                await _pageContentService.UpdatePageContentAsync(page);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("DeletePageContent")]
        public async Task<IActionResult> DeletePageContent(int pageContentId)
        {
            try
            {
                await _pageContentService.DeletePageContentAsync(pageContentId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
