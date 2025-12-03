using DiplomaFit.Api.Application.Dtos.Diet;
using DiplomaFit.Api.Application.Dtos.Workout;

namespace DiplomaFit.Api.Application.Dtos.Planning
{
    public class CombinedPlanResponseDto
    {
        public WeeklyMealPlanDto DietPlan { get; set; } = new();

        public WeeklyWorkoutPlanDto WorkoutPlan { get; set; } = new();
    }
}