namespace Booking.Application.Users.Commands.UploadProfileImage;

public class UploadProfileImageCommand
{
    public Guid UserId { get; set; }
    public byte[] ImageData { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = string.Empty;
}