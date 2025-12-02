namespace DiplomaFit.Api.Application.Interfaces
{
    public interface IMlDietPlanService
    {
        Task<Guid> GenerateOrUpdatePlanForCaseAsync(Guid caseId, CancellationToken ct = default);
    }
}
