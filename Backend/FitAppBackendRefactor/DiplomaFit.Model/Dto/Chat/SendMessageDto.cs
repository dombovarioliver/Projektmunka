using System.ComponentModel.DataAnnotations;

namespace DiplomaFit.Model.Dto.Chat
{
    public class SendMessageDto
    {
        [Required]
        public string ReceiverId { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Text { get; set; } = string.Empty;
    }
}
