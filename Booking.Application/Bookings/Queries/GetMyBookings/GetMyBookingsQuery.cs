namespace Booking.Application.Bookings.Queries.GetMyBookings;

public class GetMyBookingsQuery
{
    public Guid UserId { get; set; }
    public string? Status { get; set; }  
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}