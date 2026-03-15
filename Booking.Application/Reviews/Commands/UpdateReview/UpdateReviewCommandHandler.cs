using Booking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Booking.Application.Reviews.Commands.UpdateReview;

public class UpdateReviewCommandHandler
{
    private readonly BookingPlatformDbContext _context;

    public UpdateReviewCommandHandler(BookingPlatformDbContext context)
    {
        _context = context;
    }

    public async Task<UpdateReviewResult> Handle(UpdateReviewCommand command)
    {
        var review = await _context.Reviews
            .FirstOrDefaultAsync(r => r.Id == command.ReviewId);

        if (review == null)
        {
            return new UpdateReviewResult
            {
                IsSuccess = false,
                Error = "Review not found"
            };
        }

        // Use domain method to update
        review.UpdateReview(command.Rating, command.Comment);
        await _context.SaveChangesAsync();

        return new UpdateReviewResult
        {
            IsSuccess = true
        };
    }
}

public class UpdateReviewResult
{
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }
}