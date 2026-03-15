using Booking.Infrastructure.Data;
using Booking.Application.Reviews.Queries.GetReviewById;
using Microsoft.EntityFrameworkCore;

namespace Booking.Application.Reviews.Queries.GetReviewsByProperty;

public class GetReviewsByPropertyQueryHandler
{
    private readonly BookingPlatformDbContext _context;

    public GetReviewsByPropertyQueryHandler(BookingPlatformDbContext context)
    {
        _context = context;
    }

    public async Task<GetReviewsByPropertyResult> Handle(GetReviewsByPropertyQuery query)
    {
        var reviews = await _context.Reviews
            .Where(r => _context.Bookings
                .Any(b => b.Id == r.BookingId && b.PropertyId == query.PropertyId))
            .Select(r => new ReviewDto
            {
                Id = r.Id,
                BookingId = r.BookingId,
                GuestId = r.GuestId,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            })
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        // Calculate average rating
        var averageRating = reviews.Any()
            ? reviews.Average(r => r.Rating)
            : 0;

        return new GetReviewsByPropertyResult
        {
            IsSuccess = true,
            Reviews = reviews,
            Count = reviews.Count,
            AverageRating = averageRating
        };
    }
}

public class GetReviewsByPropertyResult
{
    public bool IsSuccess { get; set; }
    public List<ReviewDto> Reviews { get; set; }
    public int Count { get; set; }
    public double AverageRating { get; set; }
}