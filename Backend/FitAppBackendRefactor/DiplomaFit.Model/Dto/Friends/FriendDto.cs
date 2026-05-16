namespace DiplomaFit.Model.Dto.Friends
{
    public class FriendDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ProfilePictureUrl { get; set; } = string.Empty;
        public DateTime FriendsSince { get; set; }
    }
}
