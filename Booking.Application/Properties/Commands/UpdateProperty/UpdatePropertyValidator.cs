using FluentValidation;

namespace Booking.Application.Properties.Commands.UpdateProperty;

public class UpdatePropertyValidator : AbstractValidator<UpdatePropertyCommand>
{
    public UpdatePropertyValidator()
    {
        RuleFor(x => x.PropertyId)
            .NotEmpty().WithMessage("Property ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Property name is required")
            .MaximumLength(255);

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(2000);

        RuleFor(x => x.PropertyType)
            .NotEmpty().WithMessage("Property type is required")
            .MaximumLength(50);

        RuleFor(x => x.MaxGuests)
            .GreaterThan(0).WithMessage("Max guests must be greater than 0")
            .LessThanOrEqualTo(50);

        RuleFor(x => x.CheckInTime)
            .NotEmpty().WithMessage("Check-in time is required");

        RuleFor(x => x.CheckOutTime)
            .NotEmpty().WithMessage("Check-out time is required");
    }
}