using DiplomaFit.Data;
using DiplomaFit.Model.Dto.Chat;
using DiplomaFit.Model.Entities;
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
            await EnsurePrivateConversationsForFriendsAsync(userId);

            var conversations = await ctx.ChatConversations
                .AsNoTracking()
                .Include(c => c.Members)
                    .ThenInclude(m => m.User)
                .Include(c => c.Messages)
                    .ThenInclude(m => m.Sender)
                .Include(c => c.Messages)
                    .ThenInclude(m => m.Reads)
                .Where(c => c.Members.Any(m => m.UserId == userId))
                .ToListAsync();

            return conversations
                .Select(c => MapConversation(c, userId))
                .OrderByDescending(c => c.LastMessageAt ?? DateTime.MinValue)
                .ToList();
        }

        public async Task<List<ChatMessageDto>?> GetConversationMessagesAsync(string userId, string conversationId)
        {
            var conversation = await ctx.ChatConversations
                .Include(c => c.Members)
                    .ThenInclude(m => m.User)
                .Include(c => c.Messages)
                    .ThenInclude(m => m.Sender)
                .Include(c => c.Messages)
                    .ThenInclude(m => m.Reads)
                .FirstOrDefaultAsync(c => c.Id == conversationId && c.Members.Any(m => m.UserId == userId));

            if (conversation == null)
                return null;

            await MarkConversationAsReadAsync(userId, conversationId);

            return conversation.Messages
                .OrderBy(m => m.SentAt)
                .Select(m => MapMessage(m, conversation.Members))
                .ToList();
        }

        public async Task<List<ChatMessageDto>?> GetHistoryAsync(string userId, string friendId)
        {
            var conversationId = await GetOrCreatePrivateConversationAsync(userId, friendId);
            if (conversationId == null)
                return null;

            return await GetConversationMessagesAsync(userId, conversationId);
        }

        public async Task<ChatMessageDto?> SendConversationMessageAsync(string senderId, string conversationId, string text)
        {
            text = NormalizeMessageText(text);
            if (string.IsNullOrWhiteSpace(text))
                return null;

            var conversation = await ctx.ChatConversations
                .Include(c => c.Members)
                    .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(c => c.Id == conversationId && c.Members.Any(m => m.UserId == senderId));

            if (conversation == null)
                return null;

            var receiverId = conversation.IsGroup
                ? null
                : conversation.Members.FirstOrDefault(m => m.UserId != senderId)?.UserId;

            var message = new ChatMessage
            {
                Id = Guid.NewGuid().ToString(),
                ConversationId = conversation.Id,
                SenderId = senderId,
                ReceiverId = receiverId,
                Text = text,
                SentAt = DateTime.UtcNow
            };

            ctx.ChatMessages.Add(message);
            ctx.ChatMessageReads.Add(new ChatMessageRead
            {
                Id = Guid.NewGuid().ToString(),
                MessageId = message.Id,
                UserId = senderId,
                ReadAt = DateTime.UtcNow
            });

            await ctx.SaveChangesAsync();

            var savedMessage = await ctx.ChatMessages
                .AsNoTracking()
                .Include(m => m.Sender)
                .Include(m => m.Reads)
                .FirstAsync(m => m.Id == message.Id);

            return MapMessage(savedMessage, conversation.Members);
        }

        public async Task<ChatMessageDto?> SendMessageAsync(string senderId, string receiverId, string text)
        {
            var conversationId = await GetOrCreatePrivateConversationAsync(senderId, receiverId);
            if (conversationId == null)
                return null;

            return await SendConversationMessageAsync(senderId, conversationId, text);
        }

        public async Task<ConversationDto?> CreateGroupConversationAsync(string creatorId, CreateGroupConversationDto dto)
        {
            var title = (dto.Title ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(title))
                title = "Új csoport";

            if (title.Length > 120)
                title = title[..120];

            var requestedMemberIds = dto.MemberIds
                .Where(id => !string.IsNullOrWhiteSpace(id) && id != creatorId)
                .Distinct()
                .ToList();

            if (requestedMemberIds.Count == 0)
                return null;

            var allowedFriendIds = await GetAcceptedFriendIdsAsync(creatorId);
            var validMemberIds = requestedMemberIds
                .Where(id => allowedFriendIds.Contains(id))
                .ToList();

            if (validMemberIds.Count == 0)
                return null;

            var conversation = new ChatConversation
            {
                Id = Guid.NewGuid().ToString(),
                Title = title,
                IsGroup = true,
                CreatedByUserId = creatorId,
                CreatedAt = DateTime.UtcNow,
                Members = new List<ChatConversationMember>()
            };

            conversation.Members.Add(new ChatConversationMember
            {
                Id = Guid.NewGuid().ToString(),
                ConversationId = conversation.Id,
                UserId = creatorId,
                QuickEmoji = "👍",
                IsAdmin = true,
                JoinedAt = DateTime.UtcNow
            });

            foreach (var memberId in validMemberIds)
            {
                conversation.Members.Add(new ChatConversationMember
                {
                    Id = Guid.NewGuid().ToString(),
                    ConversationId = conversation.Id,
                    UserId = memberId,
                    QuickEmoji = "👍",
                    IsAdmin = false,
                    JoinedAt = DateTime.UtcNow
                });
            }

            ctx.ChatConversations.Add(conversation);
            await ctx.SaveChangesAsync();

            var savedConversation = await ctx.ChatConversations
                .AsNoTracking()
                .Include(c => c.Members)
                    .ThenInclude(m => m.User)
                .Include(c => c.Messages)
                    .ThenInclude(m => m.Reads)
                .FirstAsync(c => c.Id == conversation.Id);

            return MapConversation(savedConversation, creatorId);
        }

        public async Task<ConversationDto?> UpdateMyConversationSettingsAsync(string userId, string conversationId, UpdateConversationSettingsDto dto)
        {
            var membership = await ctx.ChatConversationMembers
                .FirstOrDefaultAsync(m => m.ConversationId == conversationId && m.UserId == userId);

            if (membership == null)
                return null;

            var nickname = (dto.Nickname ?? string.Empty).Trim();
            membership.Nickname = string.IsNullOrWhiteSpace(nickname) ? null : nickname[..Math.Min(nickname.Length, 80)];

            var quickEmoji = (dto.QuickEmoji ?? "👍").Trim();
            membership.QuickEmoji = string.IsNullOrWhiteSpace(quickEmoji) ? "👍" : quickEmoji[..Math.Min(quickEmoji.Length, 20)];

            await ctx.SaveChangesAsync();

            var conversation = await ctx.ChatConversations
                .AsNoTracking()
                .Include(c => c.Members)
                    .ThenInclude(m => m.User)
                .Include(c => c.Messages)
                    .ThenInclude(m => m.Sender)
                .Include(c => c.Messages)
                    .ThenInclude(m => m.Reads)
                .FirstAsync(c => c.Id == conversationId);

            return MapConversation(conversation, userId);
        }

        public async Task<List<string>> GetConversationMemberIdsAsync(string conversationId)
        {
            return await ctx.ChatConversationMembers
                .AsNoTracking()
                .Where(m => m.ConversationId == conversationId)
                .Select(m => m.UserId)
                .ToListAsync();
        }

        public async Task<string?> GetPrivateConversationIdAsync(string userId, string friendId)
        {
            return await GetOrCreatePrivateConversationAsync(userId, friendId);
        }

        private async Task EnsurePrivateConversationsForFriendsAsync(string userId)
        {
            var friendIds = await GetAcceptedFriendIdsAsync(userId);

            foreach (var friendId in friendIds)
            {
                await GetOrCreatePrivateConversationAsync(userId, friendId);
            }
        }

        private async Task<HashSet<string>> GetAcceptedFriendIdsAsync(string userId)
        {
            var friendIds = await ctx.Friendships
                .AsNoTracking()
                .Where(f => f.Status == FriendshipStatus.Accepted &&
                            (f.RequesterId == userId || f.AddresseeId == userId))
                .Select(f => f.RequesterId == userId ? f.AddresseeId : f.RequesterId)
                .ToListAsync();

            return friendIds.ToHashSet();
        }

        private async Task<string?> GetOrCreatePrivateConversationAsync(string userId, string friendId)
        {
            var areFriends = await AreFriendsAsync(userId, friendId);
            if (!areFriends)
                return null;

            var existingConversationId = await ctx.ChatConversations
                .AsNoTracking()
                .Where(c => !c.IsGroup &&
                            c.Members.Any(m => m.UserId == userId) &&
                            c.Members.Any(m => m.UserId == friendId))
                .Select(c => c.Id)
                .FirstOrDefaultAsync();

            if (!string.IsNullOrWhiteSpace(existingConversationId))
                return existingConversationId;

            var conversation = new ChatConversation
            {
                Id = Guid.NewGuid().ToString(),
                IsGroup = false,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow,
                Members = new List<ChatConversationMember>
                {
                    new()
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = userId,
                        QuickEmoji = "👍",
                        IsAdmin = false,
                        JoinedAt = DateTime.UtcNow
                    },
                    new()
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = friendId,
                        QuickEmoji = "👍",
                        IsAdmin = false,
                        JoinedAt = DateTime.UtcNow
                    }
                }
            };

            ctx.ChatConversations.Add(conversation);
            await ctx.SaveChangesAsync();

            await AttachOldPrivateMessagesAsync(userId, friendId, conversation.Id);

            return conversation.Id;
        }

        private async Task AttachOldPrivateMessagesAsync(string userId, string friendId, string conversationId)
        {
            var oldMessages = await ctx.ChatMessages
                .Where(m => m.ConversationId == null &&
                            ((m.SenderId == userId && m.ReceiverId == friendId) ||
                             (m.SenderId == friendId && m.ReceiverId == userId)))
                .ToListAsync();

            if (oldMessages.Count == 0)
                return;

            foreach (var message in oldMessages)
            {
                message.ConversationId = conversationId;
            }

            await ctx.SaveChangesAsync();
        }

        private async Task<bool> AreFriendsAsync(string userId, string friendId)
        {
            return await ctx.Friendships.AnyAsync(f =>
                f.Status == FriendshipStatus.Accepted &&
                ((f.RequesterId == userId && f.AddresseeId == friendId) ||
                 (f.RequesterId == friendId && f.AddresseeId == userId)));
        }

        private async Task MarkConversationAsReadAsync(string userId, string conversationId)
        {
            var unreadMessageIds = await ctx.ChatMessages
                .Where(m => m.ConversationId == conversationId &&
                            m.SenderId != userId &&
                            !m.Reads.Any(r => r.UserId == userId))
                .Select(m => m.Id)
                .ToListAsync();

            if (unreadMessageIds.Count == 0)
                return;

            foreach (var messageId in unreadMessageIds)
            {
                ctx.ChatMessageReads.Add(new ChatMessageRead
                {
                    Id = Guid.NewGuid().ToString(),
                    MessageId = messageId,
                    UserId = userId,
                    ReadAt = DateTime.UtcNow
                });
            }

            var oldPrivateUnread = await ctx.ChatMessages
                .Where(m => m.ConversationId == conversationId &&
                            m.ReceiverId == userId &&
                            m.ReadAt == null)
                .ToListAsync();

            foreach (var message in oldPrivateUnread)
            {
                message.ReadAt = DateTime.UtcNow;
            }

            await ctx.SaveChangesAsync();
        }

        private ConversationDto MapConversation(ChatConversation conversation, string currentUserId)
        {
            var orderedMessages = conversation.Messages.OrderByDescending(m => m.SentAt).ToList();
            var lastMessage = orderedMessages.FirstOrDefault();
            var currentMember = conversation.Members.FirstOrDefault(m => m.UserId == currentUserId);
            var otherMember = conversation.Members.FirstOrDefault(m => m.UserId != currentUserId);

            var members = conversation.Members
                .OrderByDescending(m => m.IsAdmin)
                .ThenBy(m => m.User!.Name)
                .Select(m => new ConversationMemberDto
                {
                    UserId = m.UserId,
                    Name = m.User?.Name ?? string.Empty,
                    Email = m.User?.Email ?? string.Empty,
                    ProfilePictureUrl = m.User?.ProfilePictureUrl ?? string.Empty,
                    DisplayName = string.IsNullOrWhiteSpace(m.Nickname) ? (m.User?.Name ?? string.Empty) : m.Nickname!,
                    Nickname = m.Nickname,
                    QuickEmoji = string.IsNullOrWhiteSpace(m.QuickEmoji) ? "👍" : m.QuickEmoji,
                    IsAdmin = m.IsAdmin
                })
                .ToList();

            var unreadCount = conversation.Messages.Count(m =>
                m.SenderId != currentUserId &&
                !m.Reads.Any(r => r.UserId == currentUserId) &&
                m.ReadAt == null);

            var displayName = conversation.IsGroup
                ? (conversation.Title ?? "Csoport")
                : (string.IsNullOrWhiteSpace(otherMember?.Nickname) ? (otherMember?.User?.Name ?? "Beszélgetés") : otherMember!.Nickname!);

            return new ConversationDto
            {
                ConversationId = conversation.Id,
                IsGroup = conversation.IsGroup,
                Title = conversation.Title ?? string.Empty,
                DisplayName = displayName,
                LastMessageText = lastMessage?.Text,
                LastMessageAt = lastMessage?.SentAt,
                UnreadCount = unreadCount,
                CurrentUserNickname = currentMember?.Nickname ?? string.Empty,
                CurrentUserQuickEmoji = string.IsNullOrWhiteSpace(currentMember?.QuickEmoji) ? "👍" : currentMember!.QuickEmoji,
                Members = members,
                FriendId = conversation.IsGroup ? string.Empty : (otherMember?.UserId ?? string.Empty),
                FriendName = conversation.IsGroup ? displayName : (string.IsNullOrWhiteSpace(otherMember?.Nickname) ? (otherMember?.User?.Name ?? string.Empty) : otherMember!.Nickname!),
                FriendEmail = conversation.IsGroup ? $"{Math.Max(0, members.Count)} tag" : (otherMember?.User?.Email ?? string.Empty),
                FriendProfilePictureUrl = conversation.IsGroup ? string.Empty : (otherMember?.User?.ProfilePictureUrl ?? string.Empty)
            };
        }

        private ChatMessageDto MapMessage(ChatMessage message, IEnumerable<ChatConversationMember> members)
        {
            var senderMember = members.FirstOrDefault(m => m.UserId == message.SenderId);
            var senderName = message.Sender?.Name ?? senderMember?.User?.Name ?? string.Empty;

            return new ChatMessageDto
            {
                Id = message.Id,
                ConversationId = message.ConversationId ?? string.Empty,
                SenderId = message.SenderId,
                ReceiverId = message.ReceiverId,
                SenderName = senderName,
                SenderDisplayName = string.IsNullOrWhiteSpace(senderMember?.Nickname) ? senderName : senderMember!.Nickname!,
                SenderProfilePictureUrl = message.Sender?.ProfilePictureUrl ?? senderMember?.User?.ProfilePictureUrl ?? string.Empty,
                Text = message.Text,
                SentAt = message.SentAt,
                ReadAt = message.ReadAt
            };
        }

        private string NormalizeMessageText(string text)
        {
            text = (text ?? string.Empty).Trim();

            if (text.Length > 2000)
                text = text[..2000];

            return text;
        }
    }
}
