namespace DiplomaFit.Api.Domain.Entities
{
    public class Meal
    {
        public Guid MealId { get; set; }
        public Guid PlanId { get; set; }
        public DietPlan Plan { get; set; } = null!;

        public MealType MealType { get; set; }
        public int MealOrder { get; set; }
        public string MealName { get; set; } = null!;

        public ICollection<MealItem> Items { get; set; } = new List<MealItem>();
    }
}
