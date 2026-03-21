using Booking.Application.Bookings.Queries.GetBookingById;
using Booking.Application.Common;
using Booking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Booking.Application.Bookings.Queries.GetBookingsForMyProperties;

public class GetBookingsForMyPropertiesQueryHandler
{
    private readonly BookingPlatformDbContext _context;

    public GetBookingsForMyPropertiesQueryHandler(BookingPlatformDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<BookingDto>> Handle(GetBookingsForMyPropertiesQuery query)
    {
        // Validate pagination
        if (query.Page < 1) query.Page = 1;
        if (query.PageSize < 1) query.PageSize = 10;
        if (query.PageSize > 100) query.PageSize = 100;

        // Build query - join with Properties to filter by OwnerId
        var bookingsQuery = _context.Bookings
            .Include(b => b.Property)
            .Where(b => b.Property.OwnerId == query.OwnerId);

        // Filter by status if provided
        if (!string.IsNullOrEmpty(query.Status))
        {
            bookingsQuery = bookingsQuery.Where(b => b.BookingStatus == query.Status);
        }

        // Get total count
        var totalCount = await bookingsQuery.CountAsync();

        // Get paginated data
        var bookings = await bookingsQuery
            .OrderByDescending(b => b.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(b => new BookingDto
            {
                Id = b.Id,
                PropertyId = b.PropertyId,
                GuestId = b.GuestId,
                StartDate = b.StartDate,
                EndDate = b.EndDate,
                GuestCount = b.GuestCount,
                CleaningFee = b.CleaningFee,
                AmenitiesUpCharge = b.AmenitiesUpCharge,
                PriceForPeriod = b.PriceForPeriod,
                TotalPrice = b.TotalPrice,
                BookingStatus = b.BookingStatus,
                CreatedAt = b.CreatedAt,
                ConfirmedOnUtc = b.ConfirmedOnUtc,
                CompletedOnUtc = b.CompletedOnUtc,
                CancelledOnUtc = b.CancelledOnUtc
            })
            .ToListAsync();

        return new PagedResult<BookingDto>(bookings, totalCount, query.Page, query.PageSize);
    }
}