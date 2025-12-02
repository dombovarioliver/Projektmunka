using System.Text.Json.Serialization;

namespace DiplomaFit.Api.Application.Dtos.Workout
{
    public class WorkoutSplitPredictionDto
    {
        [JsonPropertyName("split_type")]
        public int SplitType { get; set; }       // 0,1,2...

        [JsonPropertyName("split_name")]
        public string SplitName { get; set; } = string.Empty; // "FullBody", "UpperLower", "PushPullLegs"
    }
}
