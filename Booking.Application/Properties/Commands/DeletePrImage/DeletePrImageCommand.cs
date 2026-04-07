namespace Booking.Application.Properties.Commands.DeletePrImage;

public class DeletePrImageCommand
{
    public Guid ImageId { get; set; }
    public Guid RequestingUserId { get; set; }
}