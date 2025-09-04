
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace TweeterApp

{
    public class ChatHub : Hub
    {
        private static ConcurrentDictionary<string, string> _connections = new();
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _reactions = new();

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
            var fromUsername = Context.User.Identity.Name ?? "Anonymous";
            if (string.IsNullOrWhiteSpace(toUsername) || string.IsNullOrWhiteSpace(message))
            {
                return;
            }
            var group = DialogGroup(fromUsername, toUsername);
            var timestamp = DateTimeOffset.UtcNow;
            var id = Guid.NewGuid().ToString("N");
            await Clients.Group(group).SendAsync("ReceiveMessage", id, fromUsername, message, timestamp);


        }


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


            //var users = _reactions.GetOrAdd(key, _ => new ConcurrentDictionary<string, byte>());

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
                }
            }

        }
        public async Task SendImage(string toUsername, string imageUrl, string? caption = null)
        {
            var from = Context.User?.Identity?.Name ?? "Anonymous";
            if (string.IsNullOrEmpty(toUsername) || string.IsNullOrWhiteSpace(imageUrl))
            {
                return;
            }
            var group = DialogGroup(from, toUsername);
            var id = Guid.NewGuid().ToString("N");
            var timestamp = DateTime.UtcNow;

            await Clients.Group(group).SendAsync("ReceiveImage", id, from, imageUrl, caption, timestamp);

        }
    }
}
