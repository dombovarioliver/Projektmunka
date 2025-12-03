using DiplomaFit.Api.Application.Dtos.Cases;
using DiplomaFit.Api.Application.Dtos.Workout;

namespace DiplomaFit.Api.Application.Dtos.Planning
{
    public class CombinedPlanRequestDto
    {
        public CreateCaseRequestDto DietInputs { get; set; } = new();

        public WorkoutPlanRequestDto WorkoutInputs { get; set; } = new();
    }
}
