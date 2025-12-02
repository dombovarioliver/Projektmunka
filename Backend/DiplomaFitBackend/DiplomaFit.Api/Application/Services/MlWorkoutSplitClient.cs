using DiplomaFit.Api.Application.Dtos.Workout;
using DiplomaFit.Api.Application.Interfaces;

namespace DiplomaFit.Api.Application.Services
{
    public class MlWorkoutSplitClient : IMlWorkoutSplitClient
    {
        private readonly HttpClient _httpClient;

        public MlWorkoutSplitClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<WorkoutSplitPredictionDto> PredictSplitAsync(
            WorkoutPlanRequestDto request,
            CancellationToken ct = default)
        {
            var response = await _httpClient.PostAsJsonAsync("/workout-split/predict", request, ct);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<WorkoutSplitPredictionDto>(cancellationToken: ct);
            if (result == null)
            {
                throw new InvalidOperationException("ML workout split response was null.");
            }

            return result;
        }
    }
}
