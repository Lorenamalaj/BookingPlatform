namespace Booking.Application.Properties.Queries.GetMyProperties;

public class GetMyPropertiesQuery
{
    public Guid OwnerId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}