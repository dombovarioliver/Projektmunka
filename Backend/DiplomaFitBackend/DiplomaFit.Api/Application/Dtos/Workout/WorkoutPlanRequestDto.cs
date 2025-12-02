using System.Text.Json.Serialization;

namespace DiplomaFit.Api.Application.Dtos.Workout
{
    public class WorkoutPlanRequestDto
    {
        [JsonPropertyName("gender")]
        public int Gender { get; set; }          // 0=male, 1=female

        [JsonPropertyName("age")]
        public int Age { get; set; }

        [JsonPropertyName("goal_type")]
        public int GoalType { get; set; }        // 0=Maintain,1=Lose,2=Gain

        [JsonPropertyName("activity_level")]
        public int ActivityLevel { get; set; }   // mint a diet ML-ben

        [JsonPropertyName("experience")]
        public int Experience { get; set; }      // 1=kezdő,2=közép,3=haladó

        [JsonPropertyName("days_per_week")]
        public int DaysPerWeek { get; set; }     // 2-6

        [JsonPropertyName("equipment_level")]
        public int EquipmentLevel { get; set; }  // 0=otthon / minimál, 1=full gym
    }
}
