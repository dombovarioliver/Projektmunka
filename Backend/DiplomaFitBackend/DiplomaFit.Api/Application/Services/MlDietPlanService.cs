using DiplomaFit.Api.Application.Dtos.ML;
using DiplomaFit.Api.Application.Interfaces;
using DiplomaFit.Api.Domain.Entities;
using DiplomaFit.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DiplomaFit.Api.Application.Services
{
    public class MlDietPlanService : IMlDietPlanService
    {
        private readonly AppDbContext _db;
        private readonly IDietMlClient _mlClient;

        public MlDietPlanService(AppDbContext db, IDietMlClient mlClient)
        {
            _db = db;
            _mlClient = mlClient;
        }

        public async Task<Guid> GenerateOrUpdatePlanForCaseAsync(Guid caseId, CancellationToken ct = default)
        {
            // 1) Case lekérése
            var caseEntity = await _db.Cases
                .Include(c => c.Plan)
                .FirstOrDefaultAsync(c => c.CaseId == caseId, ct);

            if (caseEntity == null)
            {
                throw new KeyNotFoundException($"Case not found: {caseId}");
            }

            // 2) Bemeneti DTO az ML-nek
            var input = new DietInputDto
            {
                gender = caseEntity.Gender,
                age = caseEntity.Age,
                height_cm = caseEntity.HeightCm,
                weight_kg = caseEntity.WeightKg,
                bodyfat_percent = caseEntity.BodyfatPercent ?? 0,
                activity_level = caseEntity.ActivityLevel,
                goal_type = caseEntity.GoalType,
                goal_delta_kg = caseEntity.GoalDeltaKg,
                goal_time_weeks = caseEntity.GoalTimeWeeks
            };

            // 3) ML előrejelzés
            var prediction = await _mlClient.PredictAsync(input, ct);

            // 4) DietPlan létrehozása vagy frissítése
            DietPlan plan;

            if (caseEntity.Plan == null)
            {
                plan = new DietPlan
                {
                    PlanId = Guid.NewGuid()
                };

                _db.DietPlans.Add(plan);
                caseEntity.PlanId = plan.PlanId;
                caseEntity.Plan = plan;
            }
            else
            {
                plan = caseEntity.Plan;
            }

            plan.Calories = prediction.calories_kcal;
            plan.Protein = prediction.protein_g;
            plan.Carbs = prediction.carbs_g;
            plan.Fat = prediction.fat_g;
            plan.MealsPerDay = (int)Math.Round(prediction.meals_per_day);
            plan.SnacksPerDay = (int)Math.Round(prediction.snacks_per_day);

            await _db.SaveChangesAsync(ct);

            return plan.PlanId;
        }
    }
}
