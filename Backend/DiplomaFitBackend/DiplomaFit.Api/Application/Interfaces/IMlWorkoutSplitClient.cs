using DiplomaFit.Api.Application.Dtos.Workout;

namespace DiplomaFit.Api.Application.Interfaces
{
    public interface IMlWorkoutSplitClient
    {
        Task<WorkoutSplitPredictionDto> PredictSplitAsync(
            WorkoutPlanRequestDto request,
            CancellationToken ct = default);
    }
}
