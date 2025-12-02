namespace DiplomaFit.Api.Domain.Entities
{
    public class Exercise
    {
        public Guid ExerciseId { get; set; }

        public string NameHu { get; set; } = null!;
        public string NameEn { get; set; } = null!;

        public string PrimaryMuscleGroup { get; set; } = null!;      // pl. chest, back, legs, shoulders
        public string PrimaryMuscleSubgroup { get; set; } = null!;   // pl. chest_mid, legs_quads

        public string MovementType { get; set; } = null!;            // compound / isolation
        public string Pattern { get; set; } = null!;                  // squat, horizontal_press, curl...

        public string Equipment { get; set; } = null!;               // barbell, dumbbell, machine...

        public bool IsCompound { get; set; }                         // 1 = összetett, 0 = izolált
        public int DifficultyLevel { get; set; }                     // 1–3

        public string PushPullCategory { get; set; } = null!;        // Push / Pull / Legs
        public int MinExperienceLevel { get; set; }                  // 1–3

        public int DefaultSets { get; set; }
        public int DefaultRepsLow { get; set; }
        public int DefaultRepsHigh { get; set; }

        public bool IsHomeFriendly { get; set; }                     // 1 / 0
    }
}
