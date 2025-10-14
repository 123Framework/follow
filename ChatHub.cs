
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using TweeterApp.Data;
using TweeterApp.Models;

namespace TweeterApp

{
    public class ChatHub : Hub
    {
        private static ConcurrentDictionary<string, string> _connections = new();
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _reactions = new();
        private readonly ApplicationDbContext _db;
        public ChatHub(ApplicationDbContext db) => _db = db;

        private static string ReactionKey(string messageId, string emoji) => $"{messageId}::{emoji}";
        private static string UserMsgKey(string messageId, string user) => $"{messageId}::{user}";

        private static readonly ConcurrentDictionary<string, string> _userReactionByMessage = new();


        public override Task OnConnectedAsync()
        {
            var username = Context.User.Identity.Name;
            if (username != null)
            {
                _connections[username] = Context.ConnectionId;
                return base.OnConnectedAsync();
            }
            return base.OnConnectedAsync();
        }
        public override Task OnDisconnectedAsync(Exception exception)
        {
            var username = Context.User.Identity.Name;
            if (username != null)
            {
                _connections.TryRemove(username, out _);

            }
            return base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string toUsername, string message)
        {
            var me = Context.User?.Identity?.Name?.Trim().ToLowerInvariant() ?? "";
            toUsername = toUsername?.Trim().ToLowerInvariant() ?? "";
            if (string.IsNullOrWhiteSpace(toUsername)|| string.IsNullOrWhiteSpace(me) || string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            if (!await AreFriends(me, toUsername, Context.ConnectionAborted)) { return; }
            
            var group = DialogGroup(me, toUsername);
            var timestamp = DateTimeOffset.UtcNow;
            var id = Guid.NewGuid().ToString("N");
            await Clients.Group(group).SendAsync("ReceiveMessage", id, me, message, timestamp);


        }

        private Task<bool> AreFriends(string a, string b, CancellationToken ct = default) => _db.Friendships
            .AsNoTracking()
            .AnyAsync(f => f.Status == FriendshipStatus.Accepted && ((f.RequesterUserName == a && f.AddresseeUserName == b) ||  
            (f.RequesterUserName == b && f.AddresseeUserName == a)), ct);

        private static string DialogGroup(string userA, string userB)
        {
            var a = userA.Trim().ToLowerInvariant();
            var b = userB.Trim().ToLowerInvariant();
            return string.CompareOrdinal(a, b) <= 0 ? $"{a}|{b}" : $"{b}|{a}";
        }
        public async Task JoinDialog(string otherUsername)
        {
            var me = Context.User?.Identity?.Name;
            if (string.IsNullOrWhiteSpace(me) || string.IsNullOrWhiteSpace(otherUsername))
                return;

            var group = DialogGroup(me, otherUsername);
            await Groups.AddToGroupAsync(Context.ConnectionId, group);
            await Clients.Group(group).SendAsync("PresenceChanged", me, "online");
        }
        public async Task LeaveDialog(string otherUsername)
        {
            var me = Context.User?.Identity?.Name;
            if (string.IsNullOrWhiteSpace(me) || string.IsNullOrWhiteSpace(otherUsername))
                return;

            var group = DialogGroup(me, otherUsername);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);
            await Clients.Group(group).SendAsync("PresenceChanged", me, "offline");
        }
        public async Task ToggleReaction(string toUsername, string messageId, string emoji)
        {
            var me = Context.User?.Identity?.Name?.Trim();
            if (string.IsNullOrWhiteSpace(me) ||
                string.IsNullOrWhiteSpace(toUsername) ||
                string.IsNullOrWhiteSpace(messageId) ||
                string.IsNullOrWhiteSpace(emoji)) return;


            var group = DialogGroup(me, toUsername);
            var key = ReactionKey(messageId, emoji);



            var mukey = UserMsgKey(messageId, me);
            bool added;
            if (_userReactionByMessage.TryGetValue(mukey, out var existingEmoji))
            {
                if (existingEmoji == emoji)
                {
                    var users = _reactions.GetOrAdd(ReactionKey(messageId, emoji), _ => new ConcurrentDictionary<string, byte>());
                    users.TryRemove(me, out _);
                    _userReactionByMessage.TryRemove(mukey, out _);

                    var count = users.Count;
                    if (count == 0) _reactions.TryRemove(ReactionKey(messageId, emoji), out _);




                    await Clients.Group(group).SendAsync("ReactionUpdated", new
                    {
                        messageId,
                        emoji,
                        count,
                        user = me,
                        added = false,

                    });
                    return;
                }
                else
                {
                    var oldKey = ReactionKey(messageId, existingEmoji);
                    var oldUsers = _reactions.GetOrAdd(oldKey, _ => new ConcurrentDictionary<string, byte>());
                    oldUsers.TryRemove(me, out _);
                    var oldCount = oldUsers.Count;
                    if (oldCount == 0) _reactions.TryRemove(oldKey, out _);
                    await Clients.Group(group).SendAsync("ReactionUpdated", new { messageId, emoji = existingEmoji, count = oldCount, user = me, added = false });
                }
            }
            var newUsers = _reactions.GetOrAdd(ReactionKey(messageId, emoji), _ => new ConcurrentDictionary<string, byte>());
            newUsers[me] = 1;
            _userReactionByMessage[mukey] = emoji;
            var newCount = newUsers.Count();
            await Clients.Group(group).SendAsync("ReactionUpdated", new { messageId, emoji, count = newCount, user = me, added = true});
        }
        public async Task SendImage(string toUsername, string imageUrl, string? caption = null)
        {

            var me = Context.User?.Identity?.Name?.Trim().ToLowerInvariant() ?? "";
            toUsername = toUsername?.Trim().ToLowerInvariant() ?? "";
            if (string.IsNullOrWhiteSpace(toUsername) || string.IsNullOrWhiteSpace(me) || string.IsNullOrWhiteSpace(imageUrl))
            {
                return;
            }

            if (!await AreFriends(me, toUsername, Context.ConnectionAborted)) { return; }

            var group = DialogGroup(me, toUsername);
            var id = Guid.NewGuid().ToString("N");
            var timestamp = DateTime.UtcNow;

            await Clients.Group(group).SendAsync("ReceiveImage", id, me, imageUrl, caption, timestamp);

        }


    }
}
