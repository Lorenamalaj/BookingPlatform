using Booking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Booking.Application.Properties.Queries.GetPropertyById;

public class GetPropertyByIdQueryHandler
{
    private readonly BookingPlatformDbContext _context;

    public GetPropertyByIdQueryHandler(BookingPlatformDbContext context)
    {
        _context = context;
    }

    public async Task<GetPropertyByIdResult> Handle(GetPropertyByIdQuery query)
    {
        var property = await _context.Properties
            .Where(p => p.Id == query.PropertyId)
            .Select(p => new PropertyDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                PropertyType = p.PropertyType,
                MaxGuests = p.MaxGuests,
                CheckInTime = p.CheckInTime.ToString(@"hh\:mm"),
                CheckOutTime = p.CheckOutTime.ToString(@"hh\:mm"),
                IsActive = p.IsActive,
                IsApproved = p.IsApproved,
                OwnerId = p.OwnerId,
                AddressId = p.AddressId,
                CreatedAt = p.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (property == null)
        {
            return new GetPropertyByIdResult
            {
                IsSuccess = false,
                Error = "Property not found"
            };
        }

        return new GetPropertyByIdResult
        {
            IsSuccess = true,
            Property = property
        };
    }
}

public class GetPropertyByIdResult
{
    public bool IsSuccess { get; set; }
    public PropertyDto? Property { get; set; }
    public string? Error { get; set; }
}

public class PropertyDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string PropertyType { get; set; }
    public int MaxGuests { get; set; }
    public string CheckInTime { get; set; }
    public string CheckOutTime { get; set; }
    public bool IsActive { get; set; }
    public bool IsApproved { get; set; }
    public int OwnerId { get; set; }
    public int AddressId { get; set; }
    public DateTime CreatedAt { get; set; }
}