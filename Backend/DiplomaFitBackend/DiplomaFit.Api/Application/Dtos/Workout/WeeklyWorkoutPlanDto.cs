namespace DiplomaFit.Api.Application.Dtos.Workout
{
    public class WeeklyWorkoutPlanDto
    {
        public string SplitName { get; set; } = string.Empty;
        public int DaysPerWeek { get; set; }
        public List<WorkoutDayDto> Days { get; set; } = new();
    }
}
