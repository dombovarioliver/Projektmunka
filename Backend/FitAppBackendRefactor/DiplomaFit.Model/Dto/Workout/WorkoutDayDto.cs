using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiplomaFit.Model.Dto.Workout
{
    public class WorkoutDayDto
    {
        public int DayIndex { get; set; }

        public string DayType { get; set; } = string.Empty;

        public List<WorkoutExerciseDto> Exercises { get; set; } = new();
    }
}
