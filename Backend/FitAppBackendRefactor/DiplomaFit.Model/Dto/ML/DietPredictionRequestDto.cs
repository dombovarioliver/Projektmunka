using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace DiplomaFit.Model.Dto.ML
{
    public class DietPredictionRequestDto
    {
        [JsonPropertyName("gender")]
        public double Gender { get; set; }

        [JsonPropertyName("age")]
        public double Age { get; set; }

        [JsonPropertyName("height_cm")]
        public double HeightCm { get; set; }

        [JsonPropertyName("weight_kg")]
        public double WeightKg { get; set; }

        [JsonPropertyName("bodyfat_percent")]
        public double BodyfatPercent { get; set; }

        [JsonPropertyName("activity_level")]
        public double ActivityLevel { get; set; }

        [JsonPropertyName("goal_type")]
        public double GoalType { get; set; }

        [JsonPropertyName("goal_delta_kg")]
        public double GoalDeltaKg { get; set; }

        [JsonPropertyName("goal_time_weeks")]
        public double GoalTimeWeeks { get; set; }
    }
}
