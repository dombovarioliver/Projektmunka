namespace DiplomaFit.Api.Application.Dtos.Diet
{
    public class WeeklyMealPlanDto
    {
        public Guid CaseId { get; set; }
        public double DailyCalories { get; set; }
        public double DailyProtein { get; set; }
        public double DailyCarbs { get; set; }
        public double DailyFat { get; set; }

        public List<DayPlanDto> Days { get; set; } = new();
    }
}
