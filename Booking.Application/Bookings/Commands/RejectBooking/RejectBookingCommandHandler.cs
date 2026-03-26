using Booking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Booking.Application.Bookings.Commands.RejectBooking;

public class RejectBookingCommandHandler
{
    private readonly BookingPlatformDbContext _context;

    public RejectBookingCommandHandler(BookingPlatformDbContext context)
    {
        _context = context;
    }

    public async Task<RejectBookingResult> Handle(RejectBookingCommand command)
    {
        var booking = await _context.Bookings
            .Include(b => b.Property)
            .FirstOrDefaultAsync(b => b.Id == command.BookingId);

        if (booking == null)
        {
            return new RejectBookingResult { IsSuccess = false, Error = "Booking not found" };
        }

        if (booking.Property == null)
        {
            return new RejectBookingResult { IsSuccess = false, Error = "Property information not found" };
        }

        // KONTROLLI: Vetëm Owner-i i pronës mund ta bëjë REJECT (jo guest-i)
        if (booking.Property.OwnerId != command.RequestingUserId)
        {
            return new RejectBookingResult
            {
                IsSuccess = false,
                Error = "Unauthorized: Only the property owner can reject this booking"
            };
        }

        try
        {
            // Përdorim metodën e Domain-it që sapo krijuam
            booking.Reject();
            await _context.SaveChangesAsync();

            return new RejectBookingResult { IsSuccess = true };
        }
        catch (InvalidOperationException ex)
        {
            return new RejectBookingResult { IsSuccess = false, Error = ex.Message };
        }
    }
}

public class RejectBookingResult
{
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }
}