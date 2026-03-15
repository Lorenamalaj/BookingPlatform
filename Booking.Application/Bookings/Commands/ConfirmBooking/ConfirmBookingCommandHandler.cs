using Booking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Booking.Application.Bookings.Commands.ConfirmBooking;

public class ConfirmBookingCommandHandler
{
    private readonly BookingPlatformDbContext _context;

    public ConfirmBookingCommandHandler(BookingPlatformDbContext context)
    {
        _context = context;
    }

    public async Task<ConfirmBookingResult> Handle(ConfirmBookingCommand command)
    {
        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.Id == command.BookingId);

        if (booking == null)
        {
            return new ConfirmBookingResult
            {
                IsSuccess = false,
                Error = "Booking not found"
            };
        }

        try
        {
            // Domain method handles validation (can only confirm pending)
            booking.Confirm();
            await _context.SaveChangesAsync();

            return new ConfirmBookingResult
            {
                IsSuccess = true
            };
        }
        catch (InvalidOperationException ex)
        {
            return new ConfirmBookingResult
            {
                IsSuccess = false,
                Error = ex.Message
            };
        }
    }
}

public class ConfirmBookingResult
{
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }
}