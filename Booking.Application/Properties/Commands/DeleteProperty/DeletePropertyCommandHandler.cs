using Booking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Booking.Application.Properties.Commands.DeleteProperty;

public class DeletePropertyCommandHandler
{
    private readonly BookingPlatformDbContext _context;

    public DeletePropertyCommandHandler(BookingPlatformDbContext context)
    {
        _context = context;
    }

    public async Task<DeletePropertyResult> Handle(DeletePropertyCommand command)
    {
        // Validate property exists
        var property = await _context.Properties
            .FirstOrDefaultAsync(p => p.Id == command.PropertyId);

        if (property == null)
        {
            return new DeletePropertyResult
            {
                IsSuccess = false,
                Error = "Property not found"
            };
        }

        // Check if property has bookings
        var hasBookings = await _context.Bookings
            .AnyAsync(b => b.PropertyId == command.PropertyId);

        if (hasBookings)
        {
            return new DeletePropertyResult
            {
                IsSuccess = false, 
                Error = "Cannot delete property with existing bookings"
            };
        }

        // Safe to delete
        _context.Properties.Remove(property);
        await _context.SaveChangesAsync();

        return new DeletePropertyResult
        {
            IsSuccess = true
        };
    }
}

public class DeletePropertyResult
{
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }
}