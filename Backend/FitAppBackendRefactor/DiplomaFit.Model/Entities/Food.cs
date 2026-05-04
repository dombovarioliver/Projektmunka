using DiplomaFit.Model.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiplomaFit.Model.Entities
{
    public class Food
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string FoodId { get; set; } = Guid.NewGuid().ToString();

        [StringLength(250)]
        public string FoodNameHu { get; set; } = null!;

        [StringLength(250)]
        public string FoodNameEn { get; set; } = null!;

        [Range(0, 2000)]
        public double KcalPer100 { get; set; }

        [Range(0, 2000)]
        public double ProteinGPer100 { get; set; }

        [Range(0, 2000)]
        public double CarbsGPer100 { get; set; }

        [Range(0, 2000)]
        public double FatGPer100 { get; set; }

        public MealType MealType { get; set; }

    }
}
