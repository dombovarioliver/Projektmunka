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

        public string? RefreshToken { get; set; }

        public DateTime? RefreshTokenExpiresAt { get; set; }

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

        public double? BodyfatPercent { get; set; }

        public Activity ActivityLevel { get; set; }

        public Goal GoalType { get; set; } 

        [Range(-500, 500)]
        public int GoalDeltaKg { get; set; }

        [Range(0, 8)]
        public int GoalTimeWeeks { get; set; }

        public string ProfilePictureUrl { get; set; } = string.Empty;

        public virtual ICollection<Friendship> SentFriendRequests { get; set; } = new List<Friendship>();

        public virtual ICollection<Friendship> ReceivedFriendRequests { get; set; } = new List<Friendship>();

        public virtual ICollection<ChatMessage> SentMessages { get; set; } = new List<ChatMessage>();

        public virtual ICollection<ChatMessage> ReceivedMessages { get; set; } = new List<ChatMessage>();
    }
}
