using Microsoft.AspNetCore.SignalR;
using Booking.API.Hubs;

namespace Booking.API.Services;

public class NotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendToUser(Guid userId, string message)
    {
        await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", new
        {
            message = message,
            timestamp = DateTime.UtcNow
        });
    }

    public async Task SendToAllAdmins(string message)
    {
        await _hubContext.Clients.All.SendAsync("ReceiveNotification", new
        {
            message = message,
            timestamp = DateTime.UtcNow,
            isAdminNotification = true
        });
    }

    public async Task NotifyBookingCreated(Guid ownerId, Guid bookingId, string propertyName)
    {
        await SendToUser(ownerId, $"New booking received for {propertyName}");
    }

    public async Task NotifyBookingConfirmed(Guid guestId, string propertyName)
    {
        await SendToUser(guestId, $"Your booking for {propertyName} has been confirmed!");
    }

    public async Task NotifyBookingRejected(Guid guestId, string propertyName)
    {
        await SendToUser(guestId, $"Your booking for {propertyName} has been rejected.");
    }

    public async Task NotifyBookingCancelled(Guid ownerId, string propertyName)
    {
        await SendToUser(ownerId, $"A booking for {propertyName} has been cancelled.");
    }

    public async Task NotifyNewReview(Guid ownerId, string propertyName, int rating)
    {
        await SendToUser(ownerId, $"New {rating}-star review received for {propertyName}");
    }

    public async Task NotifyPropertyPendingApproval(string propertyName)
    {
        await SendToAllAdmins($"New property '{propertyName}' is pending approval");
    }
}