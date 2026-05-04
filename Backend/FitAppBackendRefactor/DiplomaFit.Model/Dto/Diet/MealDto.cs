using DiplomaFit.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiplomaFit.Model.Dto.Diet
{
    public class MealDto
    {
        public MealType MealType { get; set; }
        public MealType MealCategory { get; set; }

        public double TargetCalories { get; set; }
        public double TargetProtein { get; set; }
        public double TargetCarbs { get; set; }
        public double TargetFat { get; set; }

        public List<MealItemDto> Items { get; set; } = new();
    }
}
