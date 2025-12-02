using DiplomaFit.Api.Application.Dtos;
using DiplomaFit.Api.Application.Dtos.Diet;

namespace DiplomaFit.Api.Application.Interfaces
{
    public interface IDietPlanService
    {
        Task<WeeklyMealPlanDto> GenerateWeeklyPlanAsync(Guid caseId);
    }
}
