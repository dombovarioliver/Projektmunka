using DiplomaFit.Api.Application.Dtos.Cases;
using DiplomaFit.Api.Application.Dtos.Diet;
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
            // 1) Új case létrehozása és mentése
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
                // PlanId-t nem töltjük, majd ML után
            };

            _db.Cases.Add(caseEntity);
            await _db.SaveChangesAsync(ct);

            // 2) ML meghívása + DietPlan létrehozása / frissítése ehhez a case-hez
            await _mlDietPlanService.GenerateOrUpdatePlanForCaseAsync(caseEntity.CaseId, ct);

            // 3) Heti étrend generálása a létrehozott plan alapján
            var weeklyPlan = await _dietPlanService.GenerateWeeklyPlanAsync(caseEntity.CaseId);

            return Ok(weeklyPlan);
        }

        [HttpPost("workout/weekly-workout-plan")]
        public async Task<ActionResult<WeeklyWorkoutPlanDto>> GenerateWeeklyWorkoutPlan(
            [FromBody] WorkoutPlanRequestDto request,
            CancellationToken ct)
        {
            var result = await _workoutPlanService.GenerateWeeklyPlanAsync(request, ct);
            return Ok(result);
        }
    }
}
