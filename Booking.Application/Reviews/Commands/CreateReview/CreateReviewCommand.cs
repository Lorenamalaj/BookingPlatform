namespace Booking.Application.Reviews.Commands.CreateReview;

public class CreateReviewCommand
{
    public int BookingId { get; set; }
    public int GuestId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
}