using Booking.Application.Common;
using Booking.Application.Properties.Queries.GetPropertyById;
using Booking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Booking.Application.Properties.Queries.SearchProperties;

public class SearchPropertiesQueryHandler
{
    private readonly BookingPlatformDbContext _context;

    public SearchPropertiesQueryHandler(BookingPlatformDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<PropertyDto>> Handle(SearchPropertiesQuery query)
    {
        // Validate pagination
        if (query.Page < 1) query.Page = 1;
        if (query.PageSize < 1) query.PageSize = 10;
        if (query.PageSize > 100) query.PageSize = 100;

        // Build query with filters
        var propertiesQuery = _context.Properties
            .Include(p => p.Address)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(query.City))
        {
            propertiesQuery = propertiesQuery.Where(p => p.Address.City == query.City);
        }

        if (!string.IsNullOrEmpty(query.PropertyType))
        {
            propertiesQuery = propertiesQuery.Where(p => p.PropertyType == query.PropertyType);
        }

        if (query.MinGuests.HasValue)
        {
            propertiesQuery = propertiesQuery.Where(p => p.MaxGuests >= query.MinGuests.Value);
        }

        if (query.MaxGuests.HasValue)
        {
            propertiesQuery = propertiesQuery.Where(p => p.MaxGuests <= query.MaxGuests.Value);
        }

        if (query.IsApproved.HasValue)
        {
            propertiesQuery = propertiesQuery.Where(p => p.IsApproved == query.IsApproved.Value);
        }

        // Get total count AFTER filters, BEFORE pagination
        var totalCount = await propertiesQuery.CountAsync();

        // Apply pagination
        var properties = await propertiesQuery
            .OrderBy(p => p.Name)  // Consistent ordering
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