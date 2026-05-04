using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.Json.Serialization;

namespace DiplomaFit.Model.Dto.Workout
{
    public class WorkoutSplitPredictionDto
    {
        [JsonPropertyName("split_type")]
        public int SplitType { get; set; }

        [JsonPropertyName("split_name")]
        public string SplitName { get; set; } = string.Empty;
    }
}
