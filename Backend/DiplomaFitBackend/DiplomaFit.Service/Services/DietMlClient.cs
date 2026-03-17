using DiplomaFit.Api.Application.Dtos.ML;
using DiplomaFit.Api.Application.Interfaces;

namespace DiplomaFit.Api.Application.Services
{
    public class DietMlClient : IDietMlClient
    {
        private readonly HttpClient _httpClient;

        public DietMlClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<DietPredictionDto> PredictAsync(DietInputDto input, CancellationToken ct = default)
        {
            var response = await _httpClient.PostAsJsonAsync("/predict", input, ct);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<DietPredictionDto>(cancellationToken: ct);
            if (result == null)
                throw new InvalidOperationException("ML service returned empty response.");

            return result;
        }
    }
}
