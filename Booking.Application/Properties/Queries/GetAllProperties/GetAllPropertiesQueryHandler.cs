using Booking.Infrastructure.Data;
using Booking.Application.Properties.Queries.GetPropertyById;
using Microsoft.EntityFrameworkCore;

namespace Booking.Application.Properties.Queries.GetAllProperties;

public class GetAllPropertiesQueryHandler
{
    private readonly BookingPlatformDbContext _context;

    public GetAllPropertiesQueryHandler(BookingPlatformDbContext context)
    {
        _context = context;
    }

    public async Task<GetAllPropertiesResult> Handle(GetAllPropertiesQuery query)
    {
        var properties = await _context.Properties
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
            .ToListAsync();

        return new GetAllPropertiesResult
        {
            IsSuccess = true,
            Properties = properties,
            Count = properties.Count
        };
    }
}

public class GetAllPropertiesResult
{
    public bool IsSuccess { get; set; }
    public List<PropertyDto> Properties { get; set; }
    public int Count { get; set; }
}