using DiplomaFit.Model.Dto.Workout;
using DiplomaFit.Service.WorkoutService;
using Microsoft.AspNetCore.Mvc;

namespace FitAppBackendRefactor.Controllers
{
    [ApiController]
    [Route("api/workout-plans")]
    public class WorkoutPlansController : ControllerBase
    {
        private readonly WorkoutPlanService _workoutPlanService;

        public WorkoutPlansController(WorkoutPlanService workoutPlanService)
        {
            _workoutPlanService = workoutPlanService;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> Generate(
            WorkoutPlanRequestDto dto,
            CancellationToken cancellationToken)
        {
            var plan = await _workoutPlanService.GenerateWeeklyPlanAsync(dto, cancellationToken);

            return Ok(plan);
        }
    }
}