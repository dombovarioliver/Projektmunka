using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiplomaFit.Model.Dto.Workout
{
    public class WorkoutExerciseDto
    {
        public string ExerciseId { get; set; } = string.Empty;
        public string NameHu { get; set; } = string.Empty;

        public int Sets { get; set; }
        public int RepsLow { get; set; }
        public int RepsHigh { get; set; }
    }
}
