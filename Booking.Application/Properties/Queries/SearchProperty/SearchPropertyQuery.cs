namespace Booking.Application.Properties.Queries.SearchProperties;

public class SearchPropertiesQuery
{
    public string? City { get; set; }
    public string? PropertyType { get; set; }
    public int? MinGuests { get; set; }
    public int? MaxGuests { get; set; }
    public bool? IsApproved { get; set; }
}