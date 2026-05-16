using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiplomaFit.Model.Entities
{
    public class ChatMessage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string? ConversationId { get; set; }

        [Required]
        public string SenderId { get; set; } = string.Empty;

        public string? ReceiverId { get; set; }

        [Required]
        [StringLength(2000)]
        public string Text { get; set; } = string.Empty;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReadAt { get; set; }

        public virtual ChatConversation? Conversation { get; set; }

        public virtual User? Sender { get; set; }

        public virtual User? Receiver { get; set; }

        public virtual ICollection<ChatMessageRead> Reads { get; set; } = new List<ChatMessageRead>();
    }
}
