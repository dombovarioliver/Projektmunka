using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiplomaFit.Model.Dto.Workout
{
    public class WeeklyWorkoutPlanDto
    {
        public string SplitName { get; set; } = string.Empty;

        public int DaysPerWeek { get; set; }

        public List<WorkoutDayDto> Days { get; set; } = new();
    }
}
