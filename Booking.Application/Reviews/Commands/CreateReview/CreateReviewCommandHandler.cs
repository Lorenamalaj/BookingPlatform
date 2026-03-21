using Booking.Domain.Reviews;
using Booking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Booking.Application.Reviews.Commands.CreateReview;

public class CreateReviewCommandHandler
{
    private readonly BookingPlatformDbContext _context;

    public CreateReviewCommandHandler(BookingPlatformDbContext context)
    {
        _context = context;
    }

    public async Task<CreateReviewResult> Handle(CreateReviewCommand command)
    {
        // Validate booking exists and is completed
        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.Id == command.BookingId);

        if (booking == null)
        {
            return new CreateReviewResult
            {
                IsSuccess = false,
                Error = "Booking not found"
            };
        }

        if (!booking.CanBeReviewed())
        {
            return new CreateReviewResult
            {
                IsSuccess = false,
                Error = "Booking must be completed before it can be reviewed"
            };
        }

        // Check if guest matches
        if (booking.GuestId != command.GuestId)
        {
            return new CreateReviewResult
            {
                IsSuccess = false,
                Error = "Only the guest who made the booking can leave a review"
            };
        }

        // Check if review already exists
        var existingReview = await _context.Reviews
            .AnyAsync(r => r.BookingId == command.BookingId && r.GuestId == command.GuestId);

        if (existingReview)
        {
            return new CreateReviewResult
            {
                IsSuccess = false,
                Error = "Review already exists for this booking"
            };
        }

        // Create review using domain entity
        var review = new Review(
            command.BookingId,
            command.GuestId,
            command.Rating,
            command.Comment
        );

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        return new CreateReviewResult
        {
            IsSuccess = true,
            ReviewId = review.Id
        };
    }
}

public class CreateReviewResult
{
    public bool IsSuccess { get; set; }
    public Guid ReviewId { get; set; }
    public string? Error { get; set; }
}