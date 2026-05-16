namespace DiplomaFit.Model.Dto.Chat
{
    public class ConversationDto
    {
        public string ConversationId { get; set; } = string.Empty;
        public bool IsGroup { get; set; }
        public string Title { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? LastMessageText { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public int UnreadCount { get; set; }
        public string CurrentUserNickname { get; set; } = string.Empty;
        public string CurrentUserQuickEmoji { get; set; } = "👍";
        public List<ConversationMemberDto> Members { get; set; } = new();

        // Régi frontend kompatibilitás miatt megmarad.
        public string FriendId { get; set; } = string.Empty;
        public string FriendName { get; set; } = string.Empty;
        public string FriendEmail { get; set; } = string.Empty;
        public string FriendProfilePictureUrl { get; set; } = string.Empty;
    }
}
