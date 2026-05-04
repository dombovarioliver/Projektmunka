using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.Json.Serialization;

namespace DiplomaFit.Model.Dto.Workout
{
    public class WorkoutPlanRequestDto
    {
        [JsonPropertyName("gender")]
        public int Gender { get; set; }

        [JsonPropertyName("age")]
        public int Age { get; set; }

        [JsonPropertyName("goal_type")]
        public int GoalType { get; set; }

        [JsonPropertyName("activity_level")]
        public int ActivityLevel { get; set; }

        [JsonPropertyName("experience")]
        public int Experience { get; set; }

        [JsonPropertyName("days_per_week")]
        public int DaysPerWeek { get; set; }

        [JsonPropertyName("equipment_level")]
        public int EquipmentLevel { get; set; }
    }
}
