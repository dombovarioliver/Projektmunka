using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiplomaFit.Model.Dto.Gym
{
    public class GymDto
    {
        public string GymId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public double? Rating { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
