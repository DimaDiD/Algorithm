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
    public class PageContentController : ControllerBase
    {
        private readonly IPageContentService _pageContentService;
        public PageContentController(IPageContentService pageContentService)
        {
            _pageContentService = pageContentService;
        }

        [AllowAnonymous]
        [HttpGet("GetPageContentByMenuStatus")]
        public async Task<IActionResult> GetPageContentByMenuStatus(int pageId, int? subPageId, string codeStatus)
        {
            return Ok(await _pageContentService.GetPageContentBySettingStatusAsync(pageId, subPageId, codeStatus));
        }

        [HttpPost("CreatePageContent")]
        public async Task<IActionResult> CreatePageContent(PageContentDTO page)
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
        public async Task<IActionResult> UpdatePageContent(PageContentDTO page)
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

        [HttpGet("GetProgrammingTopics")]
        public async Task<IActionResult> GetProgrammingTopics()
        {
            return Ok(await _pageContentService.GetProgrammingTopicsAsync());
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
