namespace DiplomaFit.Api.Application.Dtos.Diet
{
    public class DayPlanDto
    {
        public int DayIndex { get; set; }          // 1-7
        public string? Name { get; set; }          // pl. "Hétfő"
        public List<MealDto> Meals { get; set; } = new();
    }
}
