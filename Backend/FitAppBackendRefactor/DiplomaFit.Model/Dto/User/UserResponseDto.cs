using DiplomaFit.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiplomaFit.Model.Dto.User
{
    public class UserResponseDto
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public Gender Gender { get; set; }

        public int Age { get; set; }

        public int HeightCm { get; set; }

        public double WeightKg { get; set; }

        public double? BodyfatPercent { get; set; }

        public Activity ActivityLevel { get; set; }

        public Goal GoalType { get; set; }

        public int GoalDeltaKg { get; set; }

        public int GoalTimeWeeks { get; set; }

        public string ProfilePictureUrl { get; set; } = string.Empty;
    }
}
