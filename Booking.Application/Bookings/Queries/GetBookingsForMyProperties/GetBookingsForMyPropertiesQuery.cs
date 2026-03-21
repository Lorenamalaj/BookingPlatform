namespace Booking.Application.Bookings.Queries.GetBookingsForMyProperties;

public class GetBookingsForMyPropertiesQuery
{
    public Guid OwnerId { get; set; }
    public string? Status { get; set; }  
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}