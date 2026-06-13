using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using HiveChat.Server.Data;
using HiveChat.Server.Models;

namespace HiveChat.Server.Hubs;

public class ChatHub : Hub
{
    private readonly ChatDbContext _context;

    public ChatHub(ChatDbContext context)
    {
        _context = context;
    }

    public async Task SendMessage(string username, string message)
    {
        var timestamp = DateTime.UtcNow;

        var msg = new Message
        {
            Username = username,
            Text = message,
            Timestamp = timestamp
        };

        _context.Messages.Add(msg);
        await _context.SaveChangesAsync();

        await Clients.All.SendAsync("ReceiveMessage", username, message, timestamp);
    }

    public override async Task OnConnectedAsync()
    {
        var history = _context.Messages.OrderBy(m => m.Timestamp).Take(30).ToList();

        await Clients.Caller.SendAsync("ReceiveHistory", history);
        await base.OnConnectedAsync();
    }
}
