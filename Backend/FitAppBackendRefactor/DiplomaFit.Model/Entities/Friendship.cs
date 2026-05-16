using DiplomaFit.Model.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiplomaFit.Model.Entities
{
    public class Friendship
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string RequesterId { get; set; } = string.Empty;

        [Required]
        public string AddresseeId { get; set; } = string.Empty;

        public FriendshipStatus Status { get; set; } = FriendshipStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? RespondedAt { get; set; }

        public virtual User? Requester { get; set; }

        public virtual User? Addressee { get; set; }
    }
}
