using System.ComponentModel.DataAnnotations;

namespace DiplomaFit.Model.Dto.Chat
{
    public class UpdateConversationSettingsDto
    {
        [StringLength(80)]
        public string? Nickname { get; set; }

        [StringLength(20)]
        public string QuickEmoji { get; set; } = "👍";
    }
}
