using System.ComponentModel.DataAnnotations;

namespace DiplomaFit.Model.Dto.Chat
{
    public class SendConversationMessageDto
    {
        [Required]
        [StringLength(2000)]
        public string Text { get; set; } = string.Empty;
    }
}
