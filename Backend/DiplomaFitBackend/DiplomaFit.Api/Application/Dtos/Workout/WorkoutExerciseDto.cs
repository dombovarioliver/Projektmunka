namespace DiplomaFit.Api.Application.Dtos.Workout
{
    public class WorkoutExerciseDto
    {
        public Guid ExerciseId { get; set; }
        public string NameHu { get; set; } = string.Empty;
        public int Sets { get; set; }
        public int RepsLow { get; set; }
        public int RepsHigh { get; set; }
    }
}
