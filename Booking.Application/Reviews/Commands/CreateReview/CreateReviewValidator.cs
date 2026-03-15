using FluentValidation;

namespace Booking.Application.Reviews.Commands.CreateReview;

public class CreateReviewValidator : AbstractValidator<CreateReviewCommand>
{
    public CreateReviewValidator()
    {
        RuleFor(x => x.BookingId)
            .GreaterThan(0).WithMessage("Booking ID is required");

        RuleFor(x => x.GuestId)
            .GreaterThan(0).WithMessage("Guest ID is required");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5");

        RuleFor(x => x.Comment)
            .MaximumLength(1000).WithMessage("Comment cannot exceed 1000 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Comment));
    }
}