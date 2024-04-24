using Microsoft.AspNetCore.Mvc;
using MMSA.BLL.Services.Interfaces;

namespace MMSA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubPageController : ControllerBase
    {
        private readonly ISubPageService _subPageService;
        public SubPageController(ISubPageService subPageService)
        {
            _subPageService = subPageService;
        }

        [HttpPost("CreateSubPage")]
        public async Task<IActionResult> CreateSubPage(string pageName, string subPageName)
        {
            try
            {
                await _subPageService.CreateSubPageAsync(pageName, subPageName);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("UpdateMenuItem")]
        public async Task<IActionResult> UpdateMenuItem(string oldTitle, string newTitle)
        {
            try
            {
                await _subPageService.UpdateMenuItemAsync(oldTitle, newTitle);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("DeleteMenuItem")]
        public async Task<IActionResult> DeleteeMenuItem(string title)
        {
            try
            {
                await _subPageService.DeleteMenuItemAsync(title);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
