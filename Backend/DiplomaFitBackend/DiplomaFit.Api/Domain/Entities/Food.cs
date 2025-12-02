namespace DiplomaFit.Api.Domain.Entities
{
    public class Food
    {
        public Guid FoodId { get; set; }
        public string FoodNameHu { get; set; } = null!;
        public string FoodNameEn { get; set; } = null!;

        public double KcalPer100 { get; set; }
        public double ProteinGPer100 { get; set; }
        public double CarbsGPer100 { get; set; }
        public double FatGPer100 { get; set; }

        public string MealType { get; set; } = "any";
        public ICollection<FoodMealType> MealTypes { get; set; } = new List<FoodMealType>();
    }

    public class FoodMealType
    {
        public Guid FoodId { get; set; }
        public MealType MealType { get; set; }

        public Food Food { get; set; } = null!;
    }
}
