namespace Booking.Application.Bookings.Commands.RejectBooking;

public class RejectBookingCommand
{
    public Guid BookingId { get; set; }
    public Guid RequestingUserId { get; set; }
}