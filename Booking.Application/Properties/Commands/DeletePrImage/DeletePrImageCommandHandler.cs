using Booking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Booking.Application.Properties.Commands.DeletePrImage;

public class DeletePrImageCommandHandler
{
    private readonly BookingPlatformDbContext _context;

    public DeletePrImageCommandHandler(BookingPlatformDbContext context)
    {
        _context = context;
    }

    public async Task<DeletePrImageResult> Handle(DeletePrImageCommand command)
    {
        var image = await _context.PropertyImages.FindAsync(command.ImageId);

        if (image == null)
        {
            return new DeletePrImageResult
            {
                IsSuccess = false,
                Error = "Image not found"
            };
        }

        var property = await _context.Properties.FindAsync(image.PropertyId);
        if (property == null || property.OwnerId != command.RequestingUserId)
        {
            return new DeletePrImageResult
            {
                IsSuccess = false,
                Error = "Unauthorized: You can only delete images from your own properties"
            };
        }

        _context.PropertyImages.Remove(image);
        await _context.SaveChangesAsync();

        return new DeletePrImageResult { IsSuccess = true };
    }
}

public class DeletePrImageResult
{
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }
}