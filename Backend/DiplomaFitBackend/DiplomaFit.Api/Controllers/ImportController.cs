using DiplomaFit.Api.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DiplomaFit.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImportController : ControllerBase
    {
        private readonly IImportService _importService;

        public ImportController(IImportService importService)
        {
            _importService = importService;
        }


        /// <param name="planPath">Plan.csv teljes elérési útja docs/...</param>
        /// <param name="casePath">Case.csv teljes elérési útja docs/...</param>
        [HttpPost("plans-and-cases")]
        public async Task<IActionResult> ImportPlansAndCases(
            [FromQuery] string planPath,
            [FromQuery] string casePath)
        {
            await _importService.ImportPlansAndCasesAsync(planPath, casePath);
            return Ok("Plans and Cases imported successfully.");
        }


        /// <param name="path">Foods.csv teljes elérési útja docs/...</param>
        [HttpPost("foods")]
        public async Task<IActionResult> ImportFoods([FromQuery] string path)
        {
            await _importService.ImportFoodsAsync(path);
            return Ok("Foods imported successfully.");
        }

        /// <param name="path">Foods.csv teljes elérési útja  docs/....</param>
        [HttpPost("exercises")]
        public async Task<IActionResult> ImportExercises([FromQuery] string path)
        {
            await _importService.ImportExercisesAsync(path);
            return Ok(new { message = "Exercises imported successfully." });
        }
    }
}
