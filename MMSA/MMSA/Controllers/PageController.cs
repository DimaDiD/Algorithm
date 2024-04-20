using Microsoft.AspNetCore.Mvc;
using MMSA.BLL.Dtos;
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
            await _pageService.CreatePageAsync(pageName);
            return Ok();
        }

        [HttpGet("GetMenuItems")]
        public async Task<IActionResult> GetMenuItems()
        {
            var pages = await _pageService.GetMenuItemsAsync();
            return Ok(pages);
        }
    }
}
