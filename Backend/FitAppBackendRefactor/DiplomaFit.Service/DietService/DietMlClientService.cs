using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DiplomaFit.Model.Dto.ML;
using System.Net.Http.Json;

namespace DiplomaFit.Service.DietService
{
    public class DietMlClientService
    {
        private readonly HttpClient _httpClient;

        public DietMlClientService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<DietPredictionResponseDto> PredictAsync(
            DietPredictionRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PostAsJsonAsync("/predict", request, cancellationToken);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<DietPredictionResponseDto>(
                cancellationToken: cancellationToken);

            if (result == null)
                throw new InvalidOperationException("Az ML API üres választ adott vissza.");

            return result;
        }
    }
}
