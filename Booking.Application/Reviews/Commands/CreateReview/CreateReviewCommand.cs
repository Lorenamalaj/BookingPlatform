namespace Booking.Application.Reviews.Commands.CreateReview;

public class CreateReviewCommand
{
    public Guid BookingId { get; set; }
    public Guid GuestId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
}