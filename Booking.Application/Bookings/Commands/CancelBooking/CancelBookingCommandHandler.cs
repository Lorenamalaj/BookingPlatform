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
            .FirstOrDefaultAsync(b => b.Id == command.BookingId);

        if (booking == null)
        {
            return new CancelBookingResult { IsSuccess = false, Error = "Booking not found" };
        }

        try
        {
            booking.Cancel();
            await _context.SaveChangesAsync();

            return new CancelBookingResult { IsSuccess = true };
        }
        catch (InvalidOperationException ex)
        {
            return new CancelBookingResult { IsSuccess = false, Error = ex.Message };
        }
    }
}

public class CancelBookingResult
{
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }
}

