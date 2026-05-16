using DiplomaFit.Model.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiplomaFit.Model.Dto.Food
{
    public class FoodCreateDto
    {
        [Required]
        [StringLength(250)]
        public string FoodNameHu { get; set; } = string.Empty;

        [Required]
        [StringLength(250)]
        public string FoodNameEn { get; set; } = string.Empty;

        [Required]
        public MealType MealType { get; set; }

        [Required]
        public FoodCategory FoodCategory { get; set; }

        [Range(1, 2000)]
        public double MinPortionGrams { get; set; } = 50;

        [Range(1, 2000)]
        public double MaxPortionGrams { get; set; } = 250;

        [Range(0, 2000)]
        public double KcalPer100 { get; set; }

        [Range(0, 2000)]
        public double ProteinGPer100 { get; set; }

        [Range(0, 2000)]
        public double CarbsGPer100 { get; set; }

        [Range(0, 2000)]
        public double FatGPer100 { get; set; }
    }
}
