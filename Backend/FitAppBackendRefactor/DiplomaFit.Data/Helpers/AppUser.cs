using DiplomaFit.Model.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiplomaFit.Data.Helpers
{
    public class AppUser : IdentityUser
    {
        [StringLength(200)]
        public required string Name { get; set; } = "";

        [StringLength(200)]
        public required string RefreshToken { get; set; } = "";
    }
}
