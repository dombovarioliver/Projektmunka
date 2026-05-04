using DiplomaFit.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiplomaFit.Model.Dto.Food
{
    public class FoodResponseDto
    {
        public string FoodId { get; set; } = string.Empty;

        public string FoodNameHu { get; set; } = string.Empty;

        public string FoodNameEn { get; set; } = string.Empty;

        public MealType MealType { get; set; }

        public double KcalPer100 { get; set; }

        public double ProteinGPer100 { get; set; }

        public double CarbsGPer100 { get; set; }

        public double FatGPer100 { get; set; }
    }
}
