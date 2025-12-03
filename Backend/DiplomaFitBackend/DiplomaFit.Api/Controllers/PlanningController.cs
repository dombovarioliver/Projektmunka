using DiplomaFit.Api.Application.Dtos.Cases;
using DiplomaFit.Api.Application.Dtos.Diet;
using DiplomaFit.Api.Application.Dtos.Planning;
using DiplomaFit.Api.Application.Dtos.Workout;
using DiplomaFit.Api.Application.Interfaces;
using DiplomaFit.Api.Domain.Entities;
using DiplomaFit.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace DiplomaFit.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlanningController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IMlDietPlanService _mlDietPlanService;
        private readonly IDietPlanService _dietPlanService;
        private readonly IWorkoutPlanService _workoutPlanService;

        public PlanningController(
            AppDbContext db,
            IMlDietPlanService mlDietPlanService,
            IDietPlanService dietPlanService,
            IWorkoutPlanService workoutPlanService)
        {
            _db = db;
            _mlDietPlanService = mlDietPlanService;
            _dietPlanService = dietPlanService;
            _workoutPlanService = workoutPlanService;
        }

        /// <summary>
        /// Új case létrehozása, ML hívás és 1 hetes étrend generálása egy lépésben.
        /// </summary>
        [HttpPost("generate-weekly-diet-plan")]
        [ProducesResponseType(typeof(WeeklyMealPlanDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<WeeklyMealPlanDto>> GenerateWeeklyPlanFromInput(
            [FromBody] CreateCaseRequestDto request,
            CancellationToken ct)
        {
            var caseId = await CreateCaseAsync(request, ct);

            // 2) ML meghívása + DietPlan létrehozása / frissítése ehhez a case-hez
            await _mlDietPlanService.GenerateOrUpdatePlanForCaseAsync(caseId, ct);

            // 3) Heti étrend generálása a létrehozott plan alapján
            var weeklyPlan = await _dietPlanService.GenerateWeeklyPlanAsync(caseId);

            return Ok(weeklyPlan);
        }

        /// <summary>
        /// Étrend és edzésterv generálása külön diet + workout inputok alapján.
        /// </summary>
        [HttpPost("generate-diet-and-workout-plans")]
        [ProducesResponseType(typeof(CombinedPlanResponseDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<CombinedPlanResponseDto>> GenerateDietAndWorkoutPlans(
            [FromBody] CombinedPlanRequestDto request,
            CancellationToken ct)
        {
            if (request?.DietInputs == null || request.WorkoutInputs == null)
            {
                return BadRequest("DietInputs és WorkoutInputs megadása kötelező.");
            }

            var caseId = await CreateCaseAsync(request.DietInputs, ct);

            await _mlDietPlanService.GenerateOrUpdatePlanForCaseAsync(caseId, ct);
            var weeklyDietPlan = await _dietPlanService.GenerateWeeklyPlanAsync(caseId);

            var weeklyWorkoutPlan = await _workoutPlanService.GenerateWeeklyPlanAsync(request.WorkoutInputs, ct);

            return Ok(new CombinedPlanResponseDto
            {
                DietPlan = weeklyDietPlan,
                WorkoutPlan = weeklyWorkoutPlan
            });
        }

        [HttpPost("workout/weekly-workout-plan")]
        public async Task<ActionResult<WeeklyWorkoutPlanDto>> GenerateWeeklyWorkoutPlan(
            [FromBody] WorkoutPlanRequestDto request,
            CancellationToken ct)
        {
            var result = await _workoutPlanService.GenerateWeeklyPlanAsync(request, ct);
            return Ok(result);
        }

        private async Task<Guid> CreateCaseAsync(CreateCaseRequestDto request, CancellationToken ct)
        {
            var caseEntity = new Case
            {
                CaseId = Guid.NewGuid(),
                Gender = request.Gender,
                Age = request.Age,
                HeightCm = request.HeightCm,
                WeightKg = request.WeightKg,
                BodyfatPercent = request.BodyfatPercent,
                ActivityLevel = request.ActivityLevel,
                GoalType = request.GoalType,
                GoalDeltaKg = request.GoalDeltaKg,
                GoalTimeWeeks = request.GoalTimeWeeks
            };

            _db.Cases.Add(caseEntity);
            await _db.SaveChangesAsync(ct);

            return caseEntity.CaseId;
        }
    }
}
