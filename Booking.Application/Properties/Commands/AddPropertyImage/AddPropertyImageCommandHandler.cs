using Booking.Domain.PropertyImages;
using Booking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Booking.Application.Properties.Commands.AddPropertyImage;

public class AddPropertyImageCommandHandler
{
    private readonly BookingPlatformDbContext _context;

    public AddPropertyImageCommandHandler(BookingPlatformDbContext context)
    {
        _context = context;
    }

    public async Task<AddPropertyImageResult> Handle(AddPropertyImageCommand command)
    {
        var property = await _context.Properties.FindAsync(command.PropertyId);

        if (property == null)
        {
            return new AddPropertyImageResult
            {
                IsSuccess = false,
                Error = "Property not found"
            };
        }

        if (command.IsPrimary)
        {
            var existingPrimaryImages = await _context.PropertyImages
                .Where(pi => pi.PropertyId == command.PropertyId && pi.IsPrimary)
                .ToListAsync();

            foreach (var img in existingPrimaryImages)
            {
                img.UnsetAsPrimary();
            }
        }

        var propertyImage = new PropertyImage(
            command.PropertyId,
            command.ImageData,
            command.ContentType,
            command.IsPrimary
        );

        _context.PropertyImages.Add(propertyImage);
        await _context.SaveChangesAsync();

        return new AddPropertyImageResult
        {
            IsSuccess = true,
            ImageId = propertyImage.Id
        };
    }
}

public class AddPropertyImageResult
{
    public bool IsSuccess { get; set; }
    public Guid ImageId { get; set; }
    public string? Error { get; set; }
}