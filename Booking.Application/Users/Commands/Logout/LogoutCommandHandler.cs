using Booking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Booking.Application.Users.Commands.Logout;

public class LogoutCommandHandler
{
    private readonly BookingPlatformDbContext _context;

    public LogoutCommandHandler(BookingPlatformDbContext context)
    {
        _context = context;
    }

    public async Task<LogoutResult> Handle(LogoutCommand command)
    {
        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == command.RefreshToken);

        if (refreshToken == null)
        {
            return new LogoutResult
            {
                IsSuccess = false,
                Error = "Invalid refresh token"
            };
        }

        // Revoke the refresh token
        refreshToken.Revoke();
        await _context.SaveChangesAsync();

        return new LogoutResult
        {
            IsSuccess = true
        };
    }
}

public class LogoutResult
{
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }
}