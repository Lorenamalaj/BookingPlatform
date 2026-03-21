namespace Booking.Application.Reviews.Commands.UpdateReview;

public class UpdateReviewCommand
{
    public Guid ReviewId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
}