namespace DiplomaFit.Api.Application.Dtos.Workout
{
    public class WorkoutDayDto
    {
        public int DayIndex { get; set; }        // 1-7
        public string DayType { get; set; } = string.Empty;   // "Push", "Pull", "Legs", "FullBody", "Rest"
        public List<WorkoutExerciseDto> Exercises { get; set; } = new();
    }
}
