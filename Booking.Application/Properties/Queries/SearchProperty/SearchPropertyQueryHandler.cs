using Booking.Infrastructure.Data;
using Booking.Application.Properties.Queries.GetPropertyById;
using Microsoft.EntityFrameworkCore;

namespace Booking.Application.Properties.Queries.SearchProperties;

public class SearchPropertiesQueryHandler
{
    private readonly BookingPlatformDbContext _context;

    public SearchPropertiesQueryHandler(BookingPlatformDbContext context)
    {
        _context = context;
    }

    public async Task<SearchPropertiesResult> Handle(SearchPropertiesQuery query)
    {
        var propertiesQuery = _context.Properties.AsQueryable();

        // Filter by City (via Address)
        if (!string.IsNullOrWhiteSpace(query.City))
        {
            propertiesQuery = propertiesQuery.Where(p =>
                _context.Addresses
                    .Any(a => a.Id == p.AddressId && a.City.Contains(query.City)));
        }

        // Filter by PropertyType
        if (!string.IsNullOrWhiteSpace(query.PropertyType))
        {
            propertiesQuery = propertiesQuery.Where(p =>
                p.PropertyType.Contains(query.PropertyType));
        }

        // Filter by MinGuests
        if (query.MinGuests.HasValue)
        {
            propertiesQuery = propertiesQuery.Where(p =>
                p.MaxGuests >= query.MinGuests.Value);
        }

        // Filter by MaxGuests
        if (query.MaxGuests.HasValue)
        {
            propertiesQuery = propertiesQuery.Where(p =>
                p.MaxGuests <= query.MaxGuests.Value);
        }

        // Filter by IsApproved
        if (query.IsApproved.HasValue)
        {
            propertiesQuery = propertiesQuery.Where(p =>
                p.IsApproved == query.IsApproved.Value);
        }

        var properties = await propertiesQuery
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

        return new SearchPropertiesResult
        {
            IsSuccess = true,
            Properties = properties,
            Count = properties.Count
        };
    }
}

public class SearchPropertiesResult
{
    public bool IsSuccess { get; set; }
    public List<PropertyDto> Properties { get; set; }
    public int Count { get; set; }
}