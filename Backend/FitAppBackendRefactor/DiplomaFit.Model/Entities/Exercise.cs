using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiplomaFit.Model.Entities
{
    public class Exercise
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string ExerciseId { get; set; } = Guid.NewGuid().ToString();

        [StringLength(250)]
        public string NameHu { get; set; } = null!;

        [StringLength(250)]
        public string NameEn { get; set; } = null!;

        [StringLength(250)]
        public string VideoUrl { get; set; } = string.Empty;

        [StringLength(250)]
        public string PrimaryMuscleGroup { get; set; } = null!;      // pl. chest, back, legs, shoulders

        [StringLength(250)]
        public string PrimaryMuscleSubgroup { get; set; } = null!;   // pl. chest_mid, legs_quads

        [StringLength(250)]
        public string MovementType { get; set; } = null!;            // compound / isolation

        [StringLength(250)]
        public string Pattern { get; set; } = null!;                  // squat, horizontal_press, curl...

        [StringLength(250)]
        public string Equipment { get; set; } = null!;               // barbell, dumbbell, machine...

        [Range(0, 2)]
        public bool IsCompound { get; set; }                         // 1 = összetett, 0 = izolált

        [Range(0, 4)]
        public int DifficultyLevel { get; set; }                     // 1–3

        [StringLength(250)]
        public string PushPullCategory { get; set; } = null!;        // Push / Pull / Legs

        [Range(0, 4)]
        public int MinExperienceLevel { get; set; }                  // 1–3

        [Range(0, 6)]
        public int DefaultSets { get; set; }

        [Range(0, 50)]
        public int DefaultRepsLow { get; set; }

        [Range(0, 100)]
        public int DefaultRepsHigh { get; set; }

        [Range(0, 2)]
        public bool IsHomeFriendly { get; set; }
    }
}
