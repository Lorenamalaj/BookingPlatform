using Booking.Infrastructure.Data;
using Booking.Application.Bookings.Queries.GetBookingById;
using Microsoft.EntityFrameworkCore;

namespace Booking.Application.Bookings.Queries.GetAllBookings;

public class GetAllBookingsQueryHandler
{
    private readonly BookingPlatformDbContext _context;

    public GetAllBookingsQueryHandler(BookingPlatformDbContext context)
    {
        _context = context;
    }

    public async Task<GetAllBookingsResult> Handle(GetAllBookingsQuery query)
    {
        var bookings = await _context.Bookings
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

        return new GetAllBookingsResult
        {
            IsSuccess = true,
            Bookings = bookings,
            Count = bookings.Count
        };
    }
}

public class GetAllBookingsResult
{
    public bool IsSuccess { get; set; }
    public List<BookingDto> Bookings { get; set; }
    public int Count { get; set; }
}