namespace DiplomaFit.Model.Dto.Friends
{
    public class FriendRequestDto
    {
        public string FriendshipId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ProfilePictureUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
