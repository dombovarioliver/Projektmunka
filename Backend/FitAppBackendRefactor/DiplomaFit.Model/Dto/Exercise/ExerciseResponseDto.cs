using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiplomaFit.Model.Dto.Exercise
{
    public class ExerciseResponseDto
    {
        public string ExerciseId { get; set; } = string.Empty;

        public string NameHu { get; set; } = string.Empty;

        public string NameEn { get; set; } = string.Empty;

        public string PrimaryMuscleGroup { get; set; } = string.Empty;

        public string PrimaryMuscleSubgroup { get; set; } = string.Empty;

        public string MovementType { get; set; } = string.Empty;

        public string Pattern { get; set; } = string.Empty;

        public string Equipment { get; set; } = string.Empty;

        public bool IsCompound { get; set; }

        public int DifficultyLevel { get; set; }

        public string PushPullCategory { get; set; } = string.Empty;

        public int MinExperienceLevel { get; set; }

        public int DefaultSets { get; set; }

        public int DefaultRepsLow { get; set; }

        public int DefaultRepsHigh { get; set; }

        public bool IsHomeFriendly { get; set; }
    }
}
