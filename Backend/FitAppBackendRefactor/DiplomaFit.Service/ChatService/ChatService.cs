using DiplomaFit.Data;
using DiplomaFit.Model.Dto.Chat;
using DiplomaFit.Model.Enums;
using Microsoft.EntityFrameworkCore;

namespace DiplomaFit.Service.ChatService
{
    public class ChatService
    {
        private readonly AppDbContext ctx;

        public ChatService(AppDbContext ctx)
        {
            this.ctx = ctx;
        }

        public async Task<List<ConversationDto>> GetConversationsAsync(string userId)
        {
            var friends = await ctx.Friendships
                .AsNoTracking()
                .Include(f => f.Requester)
                .Include(f => f.Addressee)
                .Where(f => f.Status == FriendshipStatus.Accepted &&
                            (f.RequesterId == userId || f.AddresseeId == userId))
                .ToListAsync();

            var messages = await ctx.ChatMessages
                .AsNoTracking()
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();

            return friends.Select(f =>
            {
                var friend = f.RequesterId == userId ? f.Addressee! : f.Requester!;
                var lastMessage = messages.FirstOrDefault(m =>
                    (m.SenderId == userId && m.ReceiverId == friend.Id) ||
                    (m.SenderId == friend.Id && m.ReceiverId == userId));

                return new ConversationDto
                {
                    FriendId = friend.Id,
                    FriendName = friend.Name,
                    FriendEmail = friend.Email,
                    FriendProfilePictureUrl = friend.ProfilePictureUrl ?? string.Empty,
                    LastMessageText = lastMessage?.Text,
                    LastMessageAt = lastMessage?.SentAt,
                    UnreadCount = messages.Count(m => m.SenderId == friend.Id &&
                                                      m.ReceiverId == userId &&
                                                      m.ReadAt == null)
                };
            })
            .OrderByDescending(c => c.LastMessageAt ?? DateTime.MinValue)
            .ToList();
        }

        public async Task<List<ChatMessageDto>?> GetHistoryAsync(string userId, string friendId)
        {
            var areFriends = await AreFriendsAsync(userId, friendId);
            if (!areFriends)
                return null;

            var unreadMessages = await ctx.ChatMessages
                .Where(m => m.SenderId == friendId && m.ReceiverId == userId && m.ReadAt == null)
                .ToListAsync();

            foreach (var message in unreadMessages)
            {
                message.ReadAt = DateTime.UtcNow;
            }

            if (unreadMessages.Count > 0)
                await ctx.SaveChangesAsync();

            return await ctx.ChatMessages
                .AsNoTracking()
                .Where(m => (m.SenderId == userId && m.ReceiverId == friendId) ||
                            (m.SenderId == friendId && m.ReceiverId == userId))
                .OrderBy(m => m.SentAt)
                .Select(m => new ChatMessageDto
                {
                    Id = m.Id,
                    SenderId = m.SenderId,
                    ReceiverId = m.ReceiverId,
                    Text = m.Text,
                    SentAt = m.SentAt,
                    ReadAt = m.ReadAt
                })
                .ToListAsync();
        }

        public async Task<ChatMessageDto?> SendMessageAsync(string senderId, string receiverId, string text)
        {
            text = (text ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(text))
                return null;

            if (text.Length > 2000)
                text = text[..2000];

            var areFriends = await AreFriendsAsync(senderId, receiverId);
            if (!areFriends)
                return null;

            var message = new DiplomaFit.Model.Entities.ChatMessage
            {
                Id = Guid.NewGuid().ToString(),
                SenderId = senderId,
                ReceiverId = receiverId,
                Text = text,
                SentAt = DateTime.UtcNow
            };

            ctx.ChatMessages.Add(message);
            await ctx.SaveChangesAsync();

            return new ChatMessageDto
            {
                Id = message.Id,
                SenderId = message.SenderId,
                ReceiverId = message.ReceiverId,
                Text = message.Text,
                SentAt = message.SentAt,
                ReadAt = message.ReadAt
            };
        }

        private async Task<bool> AreFriendsAsync(string userId, string friendId)
        {
            return await ctx.Friendships.AnyAsync(f =>
                f.Status == FriendshipStatus.Accepted &&
                ((f.RequesterId == userId && f.AddresseeId == friendId) ||
                 (f.RequesterId == friendId && f.AddresseeId == userId)));
        }
    }
}
