using DiplomaFit.Model.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiplomaFit.Model.Dto.Auth
{
    public class RegisterRequestDto
    {
        [Required]
        [StringLength(250)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(250)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare(nameof(Password), ErrorMessage = "A két jelszó nem egyezik.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        public Gender Gender { get; set; }

        [Required]
        [Range(1, 100)]
        public int Age { get; set; }
    }
}
