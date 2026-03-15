namespace Booking.Application.Properties.Commands.CreateProperty;

public class CreatePropertyCommand
{
    public int OwnerId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string PropertyType { get; set; }

    // Address fields (instead of AddressId to be able to check if it exists)
    public string Country { get; set; }
    public string City { get; set; }
    public string Street { get; set; }
    public string? PostalCode { get; set; }

    public int MaxGuests { get; set; }
    public string CheckInTime { get; set; }
    public string CheckOutTime { get; set; }
}