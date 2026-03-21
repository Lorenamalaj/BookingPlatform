using Booking.Infrastructure.Data;
using Booking.Application.Bookings.Queries.GetBookingById;
using Booking.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace Booking.Application.Bookings.Queries.GetAllBookings;

public class GetAllBookingsQueryHandler
{
    private readonly BookingPlatformDbContext _context;

    public GetAllBookingsQueryHandler(BookingPlatformDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<BookingDto>> Handle(GetAllBookingsQuery query)
    {
        if (query.Page < 1) query.Page = 1;
        if (query.PageSize < 1) query.PageSize = 10;
        if (query.PageSize > 100) query.PageSize = 100;

        var totalCount = await _context.Bookings.CountAsync();

        var bookings = await _context.Bookings
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