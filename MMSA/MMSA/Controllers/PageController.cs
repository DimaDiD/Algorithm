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
        public async Task<IActionResult> CreatePage(PageDTO page)
        {
            var newPage = await _pageService.CreateAsync(page);
            return Ok(newPage);
        }
    }
}
