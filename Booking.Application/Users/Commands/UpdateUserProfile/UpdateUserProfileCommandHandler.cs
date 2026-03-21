using Booking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Booking.Application.Users.Commands.UpdateUserProfile;

public class UpdateUserProfileCommandHandler
{
    private readonly BookingPlatformDbContext _context;

    public UpdateUserProfileCommandHandler(BookingPlatformDbContext context)
    {
        _context = context;
    }

    public async Task<UpdateUserProfileResult> Handle(UpdateUserProfileCommand command)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == command.UserId);

        if (user == null)
        {
            return new UpdateUserProfileResult
            {
                IsSuccess = false,
                Error = "User not found"
            };
        }

        // Update only provided fields
        if (!string.IsNullOrEmpty(command.FirstName))
        {
            user.FirstName = command.FirstName;
        }

        if (!string.IsNullOrEmpty(command.LastName))
        {
            user.LastName = command.LastName;
        }

        if (command.PhoneNumber != null)
        {
            user.UpdatePhoneNumber(command.PhoneNumber);
        }

        if (command.ProfileImageUrl != null)
        {
            user.ProfileImageUrl = command.ProfileImageUrl;
        }

        user.LastModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new UpdateUserProfileResult
        {
            IsSuccess = true
        };
    }
}

public class UpdateUserProfileResult
{
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }
}