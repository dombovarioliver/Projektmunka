namespace DiplomaFit.Api.Application.Dtos
{
    public class DietPlanResponseDto
    {
        public Guid PlanId { get; set; }
        public double Calories { get; set; }
        public double Protein { get; set; }
        public double Carbs { get; set; }
        public double Fat { get; set; }

        public List<MealDto> Meals { get; set; } = new();
    }
}
