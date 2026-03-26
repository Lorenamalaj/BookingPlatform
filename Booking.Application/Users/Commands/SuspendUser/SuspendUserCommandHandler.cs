using Booking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Booking.Application.Users.Commands.SuspendUser;

public class SuspendUserCommandHandler
{
    private readonly BookingPlatformDbContext _context;

    public SuspendUserCommandHandler(BookingPlatformDbContext context)
    {
        _context = context;
    }

    public async Task<SuspendUserResult> Handle(SuspendUserCommand command, ClaimsPrincipal currentUser)
    {
        // Kontrollo nëse është Admin
        if (!currentUser.IsInRole("Admin"))
        {
            return new SuspendUserResult
            {
                IsSuccess = false,
                Error = "Unauthorized. Only admins can suspend users."
            };
        }

        var userToSuspend = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == command.UserId);

        if (userToSuspend == null)
        {
            return new SuspendUserResult
            {
                IsSuccess = false,
                Error = "User not found."
            };
        }

        userToSuspend.isActive = false;
        await _context.SaveChangesAsync();

        return new SuspendUserResult
        {
            IsSuccess = true
        };
    }
}

public class SuspendUserResult
{
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }
}