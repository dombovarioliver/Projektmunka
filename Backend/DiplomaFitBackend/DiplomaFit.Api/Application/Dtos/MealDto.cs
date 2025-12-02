namespace DiplomaFit.Api.Application.Dtos
{
    public class MealDto
    {
        public string MealType { get; set; } = "";      // pl. "Breakfast"
        public string MealCategory { get; set; } = "";  // "breakfast" / "main" / "snack"

        public double TargetCalories { get; set; }
        public double TargetProtein { get; set; }
        public double TargetCarbs { get; set; }
        public double TargetFat { get; set; }

        public List<MealItemDto> Items { get; set; } = new();
    }
}
