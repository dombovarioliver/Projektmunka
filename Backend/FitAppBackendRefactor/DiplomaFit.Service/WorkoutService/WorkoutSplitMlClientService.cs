using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiplomaFit.Model.Dto.Workout;
using System.Net.Http.Json;

namespace DiplomaFit.Service.WorkoutService
{
    public class WorkoutSplitMlClientService
    {
        private readonly HttpClient _httpClient;

        public WorkoutSplitMlClientService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<WorkoutSplitPredictionDto> PredictSplitAsync(
            WorkoutPlanRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PostAsJsonAsync(
                "/workout-split/predict",
                request,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<WorkoutSplitPredictionDto>(
                cancellationToken: cancellationToken);

            if (result == null)
                throw new InvalidOperationException("Az ML workout split API üres választ adott vissza.");

            return result;
        }
    }
}
