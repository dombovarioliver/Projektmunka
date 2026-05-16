using DiplomaFit.Data;
using DiplomaFit.Model.Dto.Friends;
using DiplomaFit.Model.Entities;
using DiplomaFit.Model.Enums;
using Microsoft.EntityFrameworkCore;

namespace DiplomaFit.Service.FriendService
{
    public class FriendService
    {
        private readonly AppDbContext ctx;

        public FriendService(AppDbContext ctx)
        {
            this.ctx = ctx;
        }

        public async Task<List<FriendDto>> GetFriendsAsync(string userId)
        {
            var friendships = await ctx.Friendships
                .AsNoTracking()
                .Include(f => f.Requester)
                .Include(f => f.Addressee)
                .Where(f => f.Status == FriendshipStatus.Accepted &&
                            (f.RequesterId == userId || f.AddresseeId == userId))
                .OrderByDescending(f => f.RespondedAt ?? f.CreatedAt)
                .ToListAsync();

            return friendships.Select(f =>
            {
                var friend = f.RequesterId == userId ? f.Addressee! : f.Requester!;

                return new FriendDto
                {
                    UserId = friend.Id,
                    Name = friend.Name,
                    Email = friend.Email,
                    ProfilePictureUrl = friend.ProfilePictureUrl ?? string.Empty,
                    FriendsSince = f.RespondedAt ?? f.CreatedAt
                };
            }).ToList();
        }

        public async Task<List<FriendRequestDto>> GetIncomingRequestsAsync(string userId)
        {
            return await ctx.Friendships
                .AsNoTracking()
                .Include(f => f.Requester)
                .Where(f => f.AddresseeId == userId && f.Status == FriendshipStatus.Pending)
                .OrderByDescending(f => f.CreatedAt)
                .Select(f => new FriendRequestDto
                {
                    FriendshipId = f.Id,
                    UserId = f.RequesterId,
                    Name = f.Requester!.Name,
                    Email = f.Requester.Email,
                    ProfilePictureUrl = f.Requester.ProfilePictureUrl ?? string.Empty,
                    CreatedAt = f.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<List<FriendRequestDto>> GetOutgoingRequestsAsync(string userId)
        {
            return await ctx.Friendships
                .AsNoTracking()
                .Include(f => f.Addressee)
                .Where(f => f.RequesterId == userId && f.Status == FriendshipStatus.Pending)
                .OrderByDescending(f => f.CreatedAt)
                .Select(f => new FriendRequestDto
                {
                    FriendshipId = f.Id,
                    UserId = f.AddresseeId,
                    Name = f.Addressee!.Name,
                    Email = f.Addressee.Email,
                    ProfilePictureUrl = f.Addressee.ProfilePictureUrl ?? string.Empty,
                    CreatedAt = f.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<List<UserSearchDto>> SearchUsersAsync(string userId, string query)
        {
            query = (query ?? string.Empty).Trim().ToLower();

            if (query.Length < 2)
                return new List<UserSearchDto>();

            var users = await ctx.Users
                .AsNoTracking()
                .Where(u => u.Id != userId &&
                            (u.Name.ToLower().Contains(query) || u.Email.ToLower().Contains(query)))
                .OrderBy(u => u.Name)
                .Take(15)
                .ToListAsync();

            var userIds = users.Select(u => u.Id).ToList();

            var friendships = await ctx.Friendships
                .AsNoTracking()
                .Where(f => (f.RequesterId == userId && userIds.Contains(f.AddresseeId)) ||
                            (f.AddresseeId == userId && userIds.Contains(f.RequesterId)))
                .ToListAsync();

            return users.Select(u =>
            {
                var friendship = friendships.FirstOrDefault(f =>
                    (f.RequesterId == userId && f.AddresseeId == u.Id) ||
                    (f.AddresseeId == userId && f.RequesterId == u.Id));

                return new UserSearchDto
                {
                    UserId = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    ProfilePictureUrl = u.ProfilePictureUrl ?? string.Empty,
                    RelationshipStatus = GetRelationshipStatus(userId, friendship)
                };
            }).ToList();
        }

        public async Task<(bool Success, string Message)> SendRequestAsync(string requesterId, string addresseeId)
        {
            if (requesterId == addresseeId)
                return (false, "Magadnak nem küldhetsz barátkérelmet.");

            var addresseeExists = await ctx.Users.AnyAsync(u => u.Id == addresseeId);
            if (!addresseeExists)
                return (false, "A felhasználó nem található.");

            var existing = await ctx.Friendships.FirstOrDefaultAsync(f =>
                (f.RequesterId == requesterId && f.AddresseeId == addresseeId) ||
                (f.RequesterId == addresseeId && f.AddresseeId == requesterId));

            if (existing != null)
            {
                if (existing.Status == FriendshipStatus.Accepted)
                    return (false, "Már barátok vagytok.");

                if (existing.Status == FriendshipStatus.Pending)
                    return (false, "Már van függőben lévő barátkérelem.");

                existing.RequesterId = requesterId;
                existing.AddresseeId = addresseeId;
                existing.Status = FriendshipStatus.Pending;
                existing.CreatedAt = DateTime.UtcNow;
                existing.RespondedAt = null;
            }
            else
            {
                ctx.Friendships.Add(new Friendship
                {
                    Id = Guid.NewGuid().ToString(),
                    RequesterId = requesterId,
                    AddresseeId = addresseeId,
                    Status = FriendshipStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await ctx.SaveChangesAsync();
            return (true, "Barátkérelem elküldve.");
        }

        public async Task<(bool Success, string Message)> AcceptRequestAsync(string userId, string friendshipId)
        {
            var friendship = await ctx.Friendships.FirstOrDefaultAsync(f =>
                f.Id == friendshipId &&
                f.AddresseeId == userId &&
                f.Status == FriendshipStatus.Pending);

            if (friendship == null)
                return (false, "A barátkérelem nem található.");

            friendship.Status = FriendshipStatus.Accepted;
            friendship.RespondedAt = DateTime.UtcNow;

            await ctx.SaveChangesAsync();
            return (true, "Barátkérelem elfogadva.");
        }

        public async Task<(bool Success, string Message)> RejectRequestAsync(string userId, string friendshipId)
        {
            var friendship = await ctx.Friendships.FirstOrDefaultAsync(f =>
                f.Id == friendshipId &&
                f.AddresseeId == userId &&
                f.Status == FriendshipStatus.Pending);

            if (friendship == null)
                return (false, "A barátkérelem nem található.");

            friendship.Status = FriendshipStatus.Rejected;
            friendship.RespondedAt = DateTime.UtcNow;

            await ctx.SaveChangesAsync();
            return (true, "Barátkérelem elutasítva.");
        }

        public async Task<(bool Success, string Message)> RemoveFriendAsync(string userId, string friendId)
        {
            var friendship = await ctx.Friendships.FirstOrDefaultAsync(f =>
                f.Status == FriendshipStatus.Accepted &&
                ((f.RequesterId == userId && f.AddresseeId == friendId) ||
                 (f.RequesterId == friendId && f.AddresseeId == userId)));

            if (friendship == null)
                return (false, "A barát nem található.");

            ctx.Friendships.Remove(friendship);
            await ctx.SaveChangesAsync();

            return (true, "Barát törölve.");
        }

        public async Task<bool> AreFriendsAsync(string userId, string friendId)
        {
            return await ctx.Friendships.AnyAsync(f =>
                f.Status == FriendshipStatus.Accepted &&
                ((f.RequesterId == userId && f.AddresseeId == friendId) ||
                 (f.RequesterId == friendId && f.AddresseeId == userId)));
        }

        private static string GetRelationshipStatus(string currentUserId, Friendship? friendship)
        {
            if (friendship == null)
                return "none";

            if (friendship.Status == FriendshipStatus.Accepted)
                return "friends";

            if (friendship.Status == FriendshipStatus.Pending && friendship.RequesterId == currentUserId)
                return "pending_sent";

            if (friendship.Status == FriendshipStatus.Pending && friendship.AddresseeId == currentUserId)
                return "pending_received";

            return "none";
        }
    }
}
