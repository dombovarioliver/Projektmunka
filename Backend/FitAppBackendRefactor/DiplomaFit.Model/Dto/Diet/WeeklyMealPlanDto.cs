using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiplomaFit.Model.Dto.Diet
{
    public class WeeklyMealPlanDto
    {
        public string UserId { get; set; } = string.Empty;

        public double DailyCalories { get; set; }
        public double DailyProtein { get; set; }
        public double DailyCarbs { get; set; }
        public double DailyFat { get; set; }

        public int MealsPerDay { get; set; }
        public int SnacksPerDay { get; set; }

        public List<DayPlanDto> Days { get; set; } = new();
    }
}
