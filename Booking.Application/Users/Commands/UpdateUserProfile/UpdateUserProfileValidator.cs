using FluentValidation;

namespace Booking.Application.Users.Commands.UpdateUserProfile;

public class UpdateUserProfileValidator : AbstractValidator<UpdateUserProfileCommand>
{
	public UpdateUserProfileValidator()
	{
		RuleFor(x => x.UserId)
			.NotEqual(Guid.Empty)
			.WithMessage("User ID is required");

		RuleFor(x => x.FirstName)
			.MaximumLength(100)
			.When(x => !string.IsNullOrEmpty(x.FirstName))
			.WithMessage("First name cannot exceed 100 characters");

		RuleFor(x => x.LastName)
			.MaximumLength(100)
			.When(x => !string.IsNullOrEmpty(x.LastName))
			.WithMessage("Last name cannot exceed 100 characters");

		RuleFor(x => x.PhoneNumber)
			.MaximumLength(20)
			.When(x => !string.IsNullOrEmpty(x.PhoneNumber))
			.WithMessage("Phone number cannot exceed 20 characters");

		RuleFor(x => x.ProfileImageUrl)
			.MaximumLength(500)
			.When(x => !string.IsNullOrEmpty(x.ProfileImageUrl))
			.WithMessage("Profile image URL cannot exceed 500 characters");
	}
}