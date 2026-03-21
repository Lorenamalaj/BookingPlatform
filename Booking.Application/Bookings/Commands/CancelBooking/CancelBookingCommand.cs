namespace Booking.Application.Bookings.Commands.CancelBooking;

public class CancelBookingCommand
{
    public Guid BookingId { get; set; }
    public Guid RequestingUserId { get; set; }
}
