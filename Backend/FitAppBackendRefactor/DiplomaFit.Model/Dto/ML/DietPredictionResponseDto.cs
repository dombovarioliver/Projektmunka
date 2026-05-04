using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.Json.Serialization;

namespace DiplomaFit.Model.Dto.ML
{
    public class DietPredictionResponseDto
    {
        [JsonPropertyName("calories_kcal")]
        public double CaloriesKcal { get; set; }

        [JsonPropertyName("protein_g")]
        public double ProteinG { get; set; }

        [JsonPropertyName("carbs_g")]
        public double CarbsG { get; set; }

        [JsonPropertyName("fat_g")]
        public double FatG { get; set; }

        [JsonPropertyName("meals_per_day")]
        public double MealsPerDay { get; set; }

        [JsonPropertyName("snacks_per_day")]
        public double SnacksPerDay { get; set; }
    }
}
