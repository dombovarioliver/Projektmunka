using DiplomaFit.Model.Dto.Chat;
using DiplomaFit.Service.ChatService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FitAppBackendRefactor.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/chat")]
    public class ChatController : ControllerBase
    {
        private readonly ChatService chatService;

        public ChatController(ChatService chatService)
        {
            this.chatService = chatService;
        }

        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversations()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            return Ok(await chatService.GetConversationsAsync(userId));
        }

        [HttpGet("conversations/{conversationId}/messages")]
        public async Task<IActionResult> GetConversationMessages(string conversationId)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var history = await chatService.GetConversationMessagesAsync(userId, conversationId);
            if (history == null) return Forbid();

            return Ok(history);
        }

        [HttpPost("conversations/{conversationId}/messages")]
        public async Task<IActionResult> SendConversationMessage(string conversationId, SendConversationMessageDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var message = await chatService.SendConversationMessageAsync(userId, conversationId, dto.Text);
            if (message == null) return BadRequest("Az üzenet nem küldhető el ebbe a beszélgetésbe.");

            return Ok(message);
        }

        [HttpPost("groups")]
        public async Task<IActionResult> CreateGroup(CreateGroupConversationDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var conversation = await chatService.CreateGroupConversationAsync(userId, dto);
            if (conversation == null) return BadRequest("A csoport létrehozásához legalább egy barátot ki kell választani.");

            return Ok(conversation);
        }

        [HttpPatch("conversations/{conversationId}/me/settings")]
        public async Task<IActionResult> UpdateMyConversationSettings(string conversationId, UpdateConversationSettingsDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var conversation = await chatService.UpdateMyConversationSettingsAsync(userId, conversationId, dto);
            if (conversation == null) return Forbid();

            return Ok(conversation);
        }

        // Régi endpoint kompatibilitás miatt megmarad.
        [HttpGet("history/{friendId}")]
        public async Task<IActionResult> GetHistory(string friendId)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var history = await chatService.GetHistoryAsync(userId, friendId);
            if (history == null) return Forbid();

            return Ok(history);
        }

        // Régi endpoint kompatibilitás miatt megmarad.
        [HttpPost("messages")]
        public async Task<IActionResult> SendMessage(SendMessageDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var message = await chatService.SendMessageAsync(userId, dto.ReceiverId, dto.Text);
            if (message == null) return BadRequest("Az üzenet nem küldhető el. Csak barátnak lehet írni.");

            return Ok(message);
        }

        private string? GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}
