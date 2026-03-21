namespace Booking.Application.Bookings.Commands.ConfirmBooking;

public class ConfirmBookingCommand
{
    public Guid BookingId { get; set; }
    public Guid RequestingUserId { get; set; }
}