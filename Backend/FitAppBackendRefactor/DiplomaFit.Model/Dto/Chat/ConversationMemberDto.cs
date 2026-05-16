namespace DiplomaFit.Model.Dto.Chat
{
    public class ConversationMemberDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ProfilePictureUrl { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Nickname { get; set; }
        public string QuickEmoji { get; set; } = "👍";
        public bool IsAdmin { get; set; }
    }
}
