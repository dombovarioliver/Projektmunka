namespace DiplomaFit.Api.Application.Dtos.ML
{
    public class DietPredictionDto
    {
        public double calories_kcal { get; set; }
        public double protein_g { get; set; }
        public double carbs_g { get; set; }
        public double fat_g { get; set; }
        public double meals_per_day { get; set; }
        public double snacks_per_day { get; set; }
    }
}
