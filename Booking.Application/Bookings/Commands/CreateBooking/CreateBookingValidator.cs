using FluentValidation;

namespace Booking.Application.Bookings.Commands.CreateBooking;

public class CreateBookingValidator : AbstractValidator<CreateBookingCommand>
{
    public CreateBookingValidator()
    {
        RuleFor(x => x.PropertyId)
            .GreaterThan(0).WithMessage("Property ID is required");

        RuleFor(x => x.GuestId)
            .GreaterThan(0).WithMessage("Guest ID is required");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required")
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
            .WithMessage("Start date cannot be in the past");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required")
            .GreaterThan(x => x.StartDate)
            .WithMessage("End date must be after start date");

        RuleFor(x => x.GuestCount)
            .GreaterThan(0).WithMessage("Guest count must be greater than 0")
            .LessThanOrEqualTo(50).WithMessage("Guest count cannot exceed 50");

        RuleFor(x => x.PriceForPeriod)
            .GreaterThan(0).WithMessage("Price must be greater than 0");

        RuleFor(x => x.CleaningFee)
            .GreaterThanOrEqualTo(0).WithMessage("Cleaning fee cannot be negative");

        RuleFor(x => x.AmenitiesUpCharge)
            .GreaterThanOrEqualTo(0).WithMessage("Amenities charge cannot be negative");
    }
}