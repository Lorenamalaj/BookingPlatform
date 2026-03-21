using Booking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Booking.Application.Bookings.Commands.CancelBooking;

public class CancelBookingCommandHandler
{
    private readonly BookingPlatformDbContext _context;

    public CancelBookingCommandHandler(BookingPlatformDbContext context)
    {
        _context = context;
    }

    public async Task<CancelBookingResult> Handle(CancelBookingCommand command)
    {
        var booking = await _context.Bookings
            .Include(b => b.Property)
            .FirstOrDefaultAsync(b => b.Id == command.BookingId);

        if (booking == null)
        {
            return new CancelBookingResult
            {
                IsSuccess = false,
                Error = "Booking not found"
            };
        }

        if (booking.Property == null)
        {
            return new CancelBookingResult
            {
                IsSuccess = false,
                Error = "Property information not found"
            };
        }

        // Authorization: Only Guest (who made booking) or Property Owner can cancel
        bool isGuest = booking.GuestId == command.RequestingUserId;
        bool isOwner = booking.Property.OwnerId == command.RequestingUserId;

        if (!isGuest && !isOwner)
        {
            return new CancelBookingResult
            {
                IsSuccess = false,
                Error = "Unauthorized: Only the guest or property owner can cancel this booking"
            };
        }

        try
        {
            booking.Cancel();  // Domain method
            await _context.SaveChangesAsync();

            return new CancelBookingResult
            {
                IsSuccess = true
            };
        }
        catch (InvalidOperationException ex)
        {
            return new CancelBookingResult
            {
                IsSuccess = false,
                Error = ex.Message
            };
        }
    }
}

public class CancelBookingResult
{
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }
}