using System.ComponentModel.DataAnnotations;

namespace DiplomaFit.Model.Dto.Friends
{
    public class CreateFriendRequestDto
    {
        [Required]
        public string AddresseeId { get; set; } = string.Empty;
    }
}
