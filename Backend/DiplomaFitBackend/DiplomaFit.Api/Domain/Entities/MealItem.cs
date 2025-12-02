namespace DiplomaFit.Api.Domain.Entities
{
    public class MealItem
    {
        public Guid MealItemId { get; set; }
        public Guid MealId { get; set; }
        public Meal Meal { get; set; } = null!;

        public Guid FoodId { get; set; }
        public Food Food { get; set; } = null!;

        public double QuantityGrams { get; set; }
    }
}
