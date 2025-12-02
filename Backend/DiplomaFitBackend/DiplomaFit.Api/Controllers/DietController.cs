using DiplomaFit.Api.Application.Dtos;
using DiplomaFit.Api.Application.Dtos.Diet;
using DiplomaFit.Api.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DiplomaFit.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DietController : ControllerBase
    {
        private readonly IDietPlanService _dietService;

        public DietController(IDietPlanService dietService)
        {
            _dietService = dietService;
        }

        /// <summary>
        /// 1 hetes étrend generálása egy adott case-hez.
        /// </summary>
        [HttpGet("cases/{caseId}/weekly-plan")]
        public async Task<ActionResult<WeeklyMealPlanDto>> GetWeeklyPlan(Guid caseId)
        {
            var plan = await _dietService.GenerateWeeklyPlanAsync(caseId);
            return Ok(plan);
        }
    }
}
