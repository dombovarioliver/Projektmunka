namespace DiplomaFit.Model.Dto.Chat
{
    public class ConversationDto
    {
        public string FriendId { get; set; } = string.Empty;
        public string FriendName { get; set; } = string.Empty;
        public string FriendEmail { get; set; } = string.Empty;
        public string FriendProfilePictureUrl { get; set; } = string.Empty;
        public string? LastMessageText { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public int UnreadCount { get; set; }
    }
}
