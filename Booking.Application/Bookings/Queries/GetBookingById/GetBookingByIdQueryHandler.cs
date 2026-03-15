using Booking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Booking.Application.Bookings.Queries.GetBookingById;

public class GetBookingByIdQueryHandler
{
    private readonly BookingPlatformDbContext _context;

    public GetBookingByIdQueryHandler(BookingPlatformDbContext context)
    {
        _context = context;
    }

    public async Task<GetBookingByIdResult> Handle(GetBookingByIdQuery query)
    {
        var booking = await _context.Bookings
            .Where(b => b.Id == query.BookingId)
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
            .FirstOrDefaultAsync();

        if (booking == null)
        {
            return new GetBookingByIdResult
            {
                IsSuccess = false,
                Error = "Booking not found"
            };
        }

        return new GetBookingByIdResult
        {
            IsSuccess = true,
            Booking = booking
        };
    }
}

public class GetBookingByIdResult
{
    public bool IsSuccess { get; set; }
    public BookingDto? Booking { get; set; }
    public string? Error { get; set; }
}

public class BookingDto
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public int GuestId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int GuestCount { get; set; }
    public decimal CleaningFee { get; set; }
    public decimal AmenitiesUpCharge { get; set; }
    public decimal PriceForPeriod { get; set; }
    public decimal TotalPrice { get; set; }
    public string BookingStatus { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ConfirmedOnUtc { get; set; }
    public DateTime? CompletedOnUtc { get; set; }
    public DateTime? CancelledOnUtc { get; set; }
}