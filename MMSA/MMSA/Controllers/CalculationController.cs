using Microsoft.AspNetCore.Mvc;
using MMSA.BLL.Services.Interfaces;
using MMSA.DAL.Dtos;
using MMSA.DAL.Entities;
using MMSA.DAL.Repositories.Interfaces;

namespace MMSA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CalculationController : ControllerBase
    {
        private readonly ICalculationService _calculationService;
        private readonly IExcelService _excelService;
        private readonly ApplicationDbContext _context;
        private readonly IPageRepository _pageRepository;

        public CalculationController(ICalculationService calculationService, IExcelService excelService, ApplicationDbContext context, IPageRepository pageRepository)
        {
            _context = context;
            _calculationService = calculationService;
            _excelService = excelService;
            _pageRepository = pageRepository;

        }

        [HttpGet("MakeCalculation")]
        public IActionResult MakeCalculation([FromQuery] CalculationInputDto calculationInput)
        {
            var product = _calculationService.MakeCalculation(calculationInput);
            if (product == null)
            {
                return Ok(new CalculationResultDto { MU = new double[][]{ }, PlotXi = new double[] { }, PlotFXi = new double[][][] { }, Error = true  });
            }
            return Ok(product);
        }

        [HttpGet("GetFile")]
        public IActionResult GenerateExcelFile([FromQuery] TableData tableResults)
        {
            var parsedTableResults = _excelService.ParseTableResults(tableResults.CalculationResults);
            
            return Ok(_excelService.GetFile(parsedTableResults));
        }

        [HttpPut("CreatePage")]
        public async Task<ActionResult> CreatePage()
        {
            var a = new Page
            {
                PageId = 1,
                PageName = "Test2",
            };
            await _pageRepository.InsertAsync(a, true);
            return Ok(a);
        }

        [HttpGet("GetUsers")]
        public async Task<ActionResult<List<User>>> Get()
        {
            var a = await _pageRepository.GetAllAsync();
            return Ok(a);
        }
    }
}   
