using DiplomaFit.Model.Dto.Friends;
using DiplomaFit.Service.FriendService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FitAppBackendRefactor.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/friends")]
    public class FriendsController : ControllerBase
    {
        private readonly FriendService friendService;

        public FriendsController(FriendService friendService)
        {
            this.friendService = friendService;
        }

        [HttpGet]
        public async Task<IActionResult> GetFriends()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            return Ok(await friendService.GetFriendsAsync(userId));
        }

        [HttpGet("requests/incoming")]
        public async Task<IActionResult> GetIncomingRequests()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            return Ok(await friendService.GetIncomingRequestsAsync(userId));
        }

        [HttpGet("requests/outgoing")]
        public async Task<IActionResult> GetOutgoingRequests()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            return Ok(await friendService.GetOutgoingRequestsAsync(userId));
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers([FromQuery] string query)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            return Ok(await friendService.SearchUsersAsync(userId, query ?? string.Empty));
        }

        [HttpPost("requests")]
        public async Task<IActionResult> SendRequest(CreateFriendRequestDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var result = await friendService.SendRequestAsync(userId, dto.AddresseeId);
            if (!result.Success) return BadRequest(result.Message);

            return Ok(new { message = result.Message });
        }

        [HttpPost("requests/{friendshipId}/accept")]
        public async Task<IActionResult> AcceptRequest(string friendshipId)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var result = await friendService.AcceptRequestAsync(userId, friendshipId);
            if (!result.Success) return BadRequest(result.Message);

            return Ok(new { message = result.Message });
        }

        [HttpPost("requests/{friendshipId}/reject")]
        public async Task<IActionResult> RejectRequest(string friendshipId)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var result = await friendService.RejectRequestAsync(userId, friendshipId);
            if (!result.Success) return BadRequest(result.Message);

            return Ok(new { message = result.Message });
        }

        [HttpDelete("{friendId}")]
        public async Task<IActionResult> RemoveFriend(string friendId)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var result = await friendService.RemoveFriendAsync(userId, friendId);
            if (!result.Success) return NotFound(result.Message);

            return NoContent();
        }

        private string? GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}
