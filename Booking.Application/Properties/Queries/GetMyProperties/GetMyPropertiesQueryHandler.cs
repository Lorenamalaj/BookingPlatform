using Booking.Application.Common;
using Booking.Application.Properties.Queries.GetPropertyById;
using Booking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Booking.Application.Properties.Queries.GetMyProperties;

public class GetMyPropertiesQueryHandler
{
    private readonly BookingPlatformDbContext _context;

    public GetMyPropertiesQueryHandler(BookingPlatformDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<PropertyDto>> Handle(GetMyPropertiesQuery query)
    {
        // Validate pagination
        if (query.Page < 1) query.Page = 1;
        if (query.PageSize < 1) query.PageSize = 10;
        if (query.PageSize > 100) query.PageSize = 100;

        // Build query
        var propertiesQuery = _context.Properties
            .Where(p => p.OwnerId == query.OwnerId);

        // Get total count
        var totalCount = await propertiesQuery.CountAsync();

        // Get paginated data
        var properties = await propertiesQuery
            .OrderByDescending(p => p.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
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

        return new PagedResult<PropertyDto>(properties, totalCount, query.Page, query.PageSize);
    }
}