
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace TweeterApp

{
    public class ChatHub : Hub
    {
        private static ConcurrentDictionary<string, string> _connections = new();
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
            if (string.IsNullOrWhiteSpace(toUsername)||string.IsNullOrWhiteSpace(message))
            {
                return;
            }
            var group = DialogGroup(fromUsername, toUsername);
            var timestamp = DateTimeOffset.UtcNow;
            await Clients.OthersInGroup(group).SendAsync("ReceiveMessage", fromUsername, message, timestamp);

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
    }
}
