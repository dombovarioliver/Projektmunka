using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiplomaFit.Model.Entities
{
    public class ChatConversation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [StringLength(120)]
        public string? Title { get; set; }

        public bool IsGroup { get; set; }

        [Required]
        public string CreatedByUserId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual User? CreatedByUser { get; set; }

        public virtual ICollection<ChatConversationMember> Members { get; set; } = new List<ChatConversationMember>();

        public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}
