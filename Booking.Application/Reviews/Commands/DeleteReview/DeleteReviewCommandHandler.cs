using Booking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Booking.Application.Reviews.Commands.DeleteReview;

public class DeleteReviewCommandHandler
{
    private readonly BookingPlatformDbContext _context;

    public DeleteReviewCommandHandler(BookingPlatformDbContext context)
    {
        _context = context;
    }

    public async Task<DeleteReviewResult> Handle(DeleteReviewCommand command)
    {
        var review = await _context.Reviews
            .FirstOrDefaultAsync(r => r.Id == command.ReviewId);

        if (review == null)
        {
            return new DeleteReviewResult
            {
                IsSuccess = false,
                Error = "Review not found"
            };
        }

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();

        return new DeleteReviewResult
        {
            IsSuccess = true
        };
    }
}

public class DeleteReviewResult
{
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }
}