using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiplomaFit.Model.Entities
{
    public class ChatMessageRead
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string MessageId { get; set; } = string.Empty;

        [Required]
        public string UserId { get; set; } = string.Empty;

        public DateTime ReadAt { get; set; } = DateTime.UtcNow;

        public virtual ChatMessage? Message { get; set; }

        public virtual User? User { get; set; }
    }
}
