using Booking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Booking.Application.Users.Commands.ChangePassword;

public class ChangePasswordCommandHandler
{
    private readonly BookingPlatformDbContext _context;

    public ChangePasswordCommandHandler(BookingPlatformDbContext context)
    {
        _context = context;
    }

    public async Task<ChangePasswordResult> Handle(ChangePasswordCommand command)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == command.UserId);

        if (user == null)
        {
            return new ChangePasswordResult
            {
                IsSuccess = false,
                Error = "User not found"
            };
        }

        // Verify current password
        var currentPasswordHash = Convert.ToBase64String(
            System.Text.Encoding.UTF8.GetBytes(command.CurrentPassword));

        if (user.PasswordHash != currentPasswordHash)
        {
            return new ChangePasswordResult
            {
                IsSuccess = false,
                Error = "Current password is incorrect"
            };
        }

        // Hash new password
        var newPasswordHash = Convert.ToBase64String(
            System.Text.Encoding.UTF8.GetBytes(command.NewPassword));

        user.UpdatePassword(newPasswordHash);
        user.LastModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new ChangePasswordResult
        {
            IsSuccess = true
        };
    }
}

public class ChangePasswordResult
{
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }
}