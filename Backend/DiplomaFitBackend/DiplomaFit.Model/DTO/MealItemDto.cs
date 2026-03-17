namespace DiplomaFit.Api.Application.Dtos
{
    public class MealItemDto
    {
        public Guid FoodId { get; set; }
        public string FoodName { get; set; } = "";
        public double QuantityGrams { get; set; }
        public double Calories { get; set; }
        public double Protein { get; set; }
        public double Carbs { get; set; }
        public double Fat { get; set; }
    }
}
