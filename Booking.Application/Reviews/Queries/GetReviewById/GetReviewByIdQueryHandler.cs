using Booking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Booking.Application.Reviews.Queries.GetReviewById;

public class GetReviewByIdQueryHandler
{
    private readonly BookingPlatformDbContext _context;

    public GetReviewByIdQueryHandler(BookingPlatformDbContext context)
    {
        _context = context;
    }

    public async Task<GetReviewByIdResult> Handle(GetReviewByIdQuery query)
    {
        var review = await _context.Reviews
            .Where(r => r.Id == query.ReviewId)
            .Select(r => new ReviewDto
            {
                Id = r.Id,
                BookingId = r.BookingId,
                GuestId = r.GuestId,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (review == null)
        {
            return new GetReviewByIdResult
            {
                IsSuccess = false,
                Error = "Review not found"
            };
        }

        return new GetReviewByIdResult
        {
            IsSuccess = true,
            Review = review
        };
    }
}

public class GetReviewByIdResult
{
    public bool IsSuccess { get; set; }
    public ReviewDto? Review { get; set; }
    public string? Error { get; set; }
}

public class ReviewDto
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public int GuestId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}