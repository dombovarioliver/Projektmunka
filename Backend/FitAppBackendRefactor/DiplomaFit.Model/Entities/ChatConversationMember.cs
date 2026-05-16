using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiplomaFit.Model.Entities
{
    public class ChatConversationMember
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string ConversationId { get; set; } = string.Empty;

        [Required]
        public string UserId { get; set; } = string.Empty;

        [StringLength(80)]
        public string? Nickname { get; set; }

        [StringLength(20)]
        public string QuickEmoji { get; set; } = "👍";

        public bool IsAdmin { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        public virtual ChatConversation? Conversation { get; set; }

        public virtual User? User { get; set; }
    }
}
