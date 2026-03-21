using Booking.Infrastructure.Data;
using Booking.Application.Properties.Queries.GetPropertyById;
using Booking.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace Booking.Application.Properties.Queries.GetAllProperties;

public class GetAllPropertiesQueryHandler
{
    private readonly BookingPlatformDbContext _context;

    public GetAllPropertiesQueryHandler(BookingPlatformDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<PropertyDto>> Handle(GetAllPropertiesQuery query)
    {

        if (query.Page < 1) query.Page = 1;
        if (query.PageSize < 1) query.PageSize = 10;
        if (query.PageSize > 100) query.PageSize = 100; // Max 100 per page

        var totalCount = await _context.Properties.CountAsync();


        var properties = await _context.Properties
            .OrderBy(p => p.CreatedAt)  // ← IMPORTANT: Consistent ordering!
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