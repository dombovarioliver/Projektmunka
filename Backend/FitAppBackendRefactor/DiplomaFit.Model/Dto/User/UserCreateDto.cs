using DiplomaFit.Model.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiplomaFit.Model.Dto.User
{
    public class UserCreateDto
    {
        [Required]
        [StringLength(250)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(250)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public Gender Gender { get; set; }

        [Required]
        [Range(0, 100)]
        public int Age { get; set; }

        [Required]
        [Range(0, 250)]
        public int HeightCm { get; set; }

        [Required]
        [Range(0, 500)]
        public double WeightKg { get; set; }

        [Range(0, 70)]
        public double? BodyfatPercent { get; set; }

       
        public Activity ActivityLevel { get; set; }

        
        public Goal GoalType { get; set; }

        
        [Range(-500, 500)]
        public int GoalDeltaKg { get; set; }

        
        [Range(0, 520)]
        public int GoalTimeWeeks { get; set; }
    }
}
