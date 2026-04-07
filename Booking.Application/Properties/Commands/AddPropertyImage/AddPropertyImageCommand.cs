namespace Booking.Application.Properties.Commands.AddPropertyImage;

public class AddPropertyImageCommand
{
    public Guid PropertyId { get; set; }
    public byte[] ImageData { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
}