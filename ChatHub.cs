
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
            if (_connections.TryGetValue(toUsername, out var connectionId))
            {
                await Clients.Client(connectionId).SendAsync("ReceiveMessage", fromUsername, message);
            }

        }
    }
}
