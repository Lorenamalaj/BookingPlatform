using Booking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Booking.Application.Properties.Commands.ApproveProperty;

public class ApprovePropertyCommandHandler
{
    private readonly BookingPlatformDbContext _context;

    public ApprovePropertyCommandHandler(BookingPlatformDbContext context)
    {
        _context = context;
    }

    public async Task<ApprovePropertyResult> Handle(ApprovePropertyCommand command)
    {
        var property = await _context.Properties
            .FirstOrDefaultAsync(p => p.Id == command.PropertyId);

        if (property == null)
        {
            return new ApprovePropertyResult
            {
                IsSuccess = false,
                Error = "Property not found"
            };
        }

        property.Approve();
        await _context.SaveChangesAsync();

        return new ApprovePropertyResult
        {
            IsSuccess = true
        };
    }
}

public class ApprovePropertyResult
{
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }
}