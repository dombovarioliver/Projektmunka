using DiplomaFit.Service.ChatService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace FitAppBackendRefactor.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ChatService chatService;

        public ChatHub(ChatService chatService)
        {
            this.chatService = chatService;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrWhiteSpace(userId))
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");

            await base.OnConnectedAsync();
        }

        public async Task SendMessage(string receiverId, string text)
        {
            var senderId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(senderId))
                throw new HubException("Nincs bejelentkezett felhasználó.");

            var message = await chatService.SendMessageAsync(senderId, receiverId, text);

            if (message == null)
                throw new HubException("Az üzenet nem küldhető el. Csak barátnak lehet írni.");

            await Clients.Group($"user-{receiverId}").SendAsync("ReceiveMessage", message);
            await Clients.Caller.SendAsync("MessageSent", message);
        }
    }
}
