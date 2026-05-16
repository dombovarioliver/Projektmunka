using System.ComponentModel.DataAnnotations;

namespace DiplomaFit.Model.Dto.Chat
{
    public class CreateGroupConversationDto
    {
        [Required]
        [StringLength(120)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public List<string> MemberIds { get; set; } = new();
    }
}
