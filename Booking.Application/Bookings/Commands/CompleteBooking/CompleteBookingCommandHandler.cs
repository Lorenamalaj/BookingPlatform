using Booking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Booking.Application.Bookings.Commands.CompleteBooking;

public class CompleteBookingCommandHandler
{
    private readonly BookingPlatformDbContext _context;

    public CompleteBookingCommandHandler(BookingPlatformDbContext context)
    {
        _context = context;
    }

    public async Task<CompleteBookingResult> Handle(CompleteBookingCommand command)
    {
        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.Id == command.BookingId);

        if (booking == null)
        {
            return new CompleteBookingResult
            {
                IsSuccess = false,
                Error = "Booking not found"
            };
        }

        try
        {
            // Domain method validates: must be confirmed and past end date
            booking.Complete();
            await _context.SaveChangesAsync();

            return new CompleteBookingResult
            {
                IsSuccess = true
            };
        }
        catch (InvalidOperationException ex)
        {
            return new CompleteBookingResult
            {
                IsSuccess = false,
                Error = ex.Message
            };
        }
    }
}

public class CompleteBookingResult
{
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }
}