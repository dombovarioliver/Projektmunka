using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiplomaFit.Model.Dto.Diet
{
    public class DayPlanDto
    {
        public int DayIndex { get; set; }
        public string Name { get; set; } = string.Empty;

        public List<MealDto> Meals { get; set; } = new();
    }
}
