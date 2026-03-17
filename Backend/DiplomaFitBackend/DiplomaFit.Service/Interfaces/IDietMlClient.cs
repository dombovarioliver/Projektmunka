using DiplomaFit.Api.Application.Dtos.ML;

namespace DiplomaFit.Api.Application.Interfaces
{
    public interface IDietMlClient
    {
        Task<DietPredictionDto> PredictAsync(DietInputDto input, CancellationToken ct = default);
    }
}
