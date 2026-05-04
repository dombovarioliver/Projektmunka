using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiplomaFit.Model.Dto.Exercise
{
    public class ExerciseCreateDto
    {
        [Required]
        [StringLength(250)]
        public string NameHu { get; set; } = string.Empty;

        [Required]
        [StringLength(250)]
        public string NameEn { get; set; } = string.Empty;

        [Required]
        [StringLength(250)]
        public string PrimaryMuscleGroup { get; set; } = string.Empty;

        [Required]
        [StringLength(250)]
        public string PrimaryMuscleSubgroup { get; set; } = string.Empty;

        [Required]
        [StringLength(250)]
        public string MovementType { get; set; } = string.Empty;

        [Required]
        [StringLength(250)]
        public string Pattern { get; set; } = string.Empty;

        [Required]
        [StringLength(250)]
        public string Equipment { get; set; } = string.Empty;

        public bool IsCompound { get; set; }

        [Range(1, 3)]
        public int DifficultyLevel { get; set; }

        [Required]
        [StringLength(250)]
        public string PushPullCategory { get; set; } = string.Empty;

        [Range(1, 3)]
        public int MinExperienceLevel { get; set; }

        [Range(1, 6)]
        public int DefaultSets { get; set; }

        [Range(1, 50)]
        public int DefaultRepsLow { get; set; }

        [Range(1, 100)]
        public int DefaultRepsHigh { get; set; }

        public bool IsHomeFriendly { get; set; }
    }
}
