using DiplomaFit.Api.Application.Dtos.Workout;

namespace DiplomaFit.Api.Application.Interfaces
{
    public interface IWorkoutPlanService
    {
        Task<WeeklyWorkoutPlanDto> GenerateWeeklyPlanAsync(
            WorkoutPlanRequestDto request,
            CancellationToken ct = default);
    }
}
