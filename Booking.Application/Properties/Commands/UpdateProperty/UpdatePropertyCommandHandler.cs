using Booking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Booking.Application.Properties.Commands.UpdateProperty;

public class UpdatePropertyCommandHandler
{
    private readonly BookingPlatformDbContext _context;

    public UpdatePropertyCommandHandler(BookingPlatformDbContext context)
    {
        _context = context;
    }

    public async Task<UpdatePropertyResult> Handle(UpdatePropertyCommand command)
    {
        // Find property
        var property = await _context.Properties
            .FirstOrDefaultAsync(p => p.Id == command.PropertyId);

        if (property == null)
        {
            return new UpdatePropertyResult
            {
                IsSuccess = false,
                Error = "Property not found"
            };
        }

        // Parse times
        var checkIn = TimeSpan.Parse(command.CheckInTime);
        var checkOut = TimeSpan.Parse(command.CheckOutTime);

        // Update using domain methods
        property.UpdateDetails(command.Name, command.Description, command.MaxGuests);
        property.UpdateCheckInCheckOut(checkIn, checkOut);

        await _context.SaveChangesAsync();

        return new UpdatePropertyResult
        {
            IsSuccess = true
        };
    }
}

public class UpdatePropertyResult
{
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }
}