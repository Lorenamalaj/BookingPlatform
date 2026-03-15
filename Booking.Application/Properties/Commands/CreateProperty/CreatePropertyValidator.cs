using FluentValidation;

namespace Booking.Application.Properties.Commands.CreateProperty;

public class CreatePropertyValidator : AbstractValidator<CreatePropertyCommand>
{
    public CreatePropertyValidator()
    {
        RuleFor(x => x.OwnerId)
            .GreaterThan(0).WithMessage("Owner ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Property name is required")
            .MaximumLength(255).WithMessage("Name cannot exceed 255 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters");

        RuleFor(x => x.PropertyType)
            .NotEmpty().WithMessage("Property type is required")
            .MaximumLength(50).WithMessage("Property type cannot exceed 50 characters");

        // Address fields
        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Country is required")
            .MaximumLength(100);

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required")
            .MaximumLength(100);

        RuleFor(x => x.Street)
            .NotEmpty().WithMessage("Street is required")
            .MaximumLength(255);

        RuleFor(x => x.PostalCode)
            .MaximumLength(20)
            .When(x => !string.IsNullOrWhiteSpace(x.PostalCode));

        RuleFor(x => x.MaxGuests)
            .GreaterThan(0).WithMessage("Max guests must be greater than 0")
            .LessThanOrEqualTo(50).WithMessage("Max guests cannot exceed 50");

        RuleFor(x => x.CheckInTime)
            .NotEmpty().WithMessage("Check-in time is required");

        RuleFor(x => x.CheckOutTime)
            .NotEmpty().WithMessage("Check-out time is required");
    }
}