using DiplomaFit.Model.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace DiplomaFit.Model.Entities
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(250)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(250)]
        public string Email { get; set; }

        [StringLength(250)]
        public string? PasswordHash { get; set; }

        public Gender Gender { get; set; }

        [Required]
        [Range(0,100)]
        public int Age { get; set; }

        [Required]
        [Range(0,250)]
        public int HeightCm { get; set; }

        [Required]
        [Range(0,500)]
        public double WeightKg { get; set; }

        [Range(0,70)]
        public double? BodyfatPercent { get; set; }

        [Required]
        public Activity ActivityLevel { get; set; }

        [Required]
        public Goal GoalType { get; set; } 

        [Required]
        [Range(-500, 500)]
        public int GoalDeltaKg { get; set; }

        [Required]
        [Range(0, 8)]
        public int GoalTimeWeeks { get; set; }
    }
}
