using Microsoft.AspNetCore.SignalR;

namespace Booking.API.Hubs;

public class NotificationHub : Hub
{
    public async Task SendNotificationToUser(string userId, string message)
    {
        await Clients.User(userId).SendAsync("ReceiveNotification", message);
    }

    public async Task SendNotificationToAll(string message)
    {
        await Clients.All.SendAsync("ReceiveNotification", message);
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            Console.WriteLine($"User {userId} connected to SignalR");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
            Console.WriteLine($"User {userId} disconnected from SignalR");
        }

        await base.OnDisconnectedAsync(exception);
    }
}