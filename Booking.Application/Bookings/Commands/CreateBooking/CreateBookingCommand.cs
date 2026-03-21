namespace Booking.Application.Bookings.Commands.CreateBooking;

public class CreateBookingCommand
{
    public Guid PropertyId { get; set; }
    public Guid GuestId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int GuestCount { get; set; }
    public decimal CleaningFee { get; set; }
    public decimal AmenitiesUpCharge { get; set; }
    public decimal PriceForPeriod { get; set; }
}