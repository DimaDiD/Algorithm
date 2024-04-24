using Microsoft.AspNetCore.Mvc;
using MMSA.BLL.Services.Interfaces;

namespace MMSA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PageController : ControllerBase
    {
        private readonly IPageService _pageService;
        public PageController(IPageService pageService)
        {
            _pageService = pageService;
        }

        [HttpPost("CreatePage")]
        public async Task<IActionResult> CreatePage(string pageName)
        {
            try
            {
                await _pageService.CreatePageAsync(pageName);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetMenuItems")]
        public async Task<IActionResult> GetMenuItems()
        {
            return Ok(await _pageService.GetMenuItemsAsync());
        }
    }
}
