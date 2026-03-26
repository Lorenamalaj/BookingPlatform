using Booking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Booking.Application.Users.Commands.DeleteUser;

public class DeleteUserCommandHandler
{
    private readonly BookingPlatformDbContext _context;

    public DeleteUserCommandHandler(BookingPlatformDbContext context)
    {
        _context = context;
    }

    public async Task<DeleteUserResult> Handle(DeleteUserCommand command)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == command.UserId);

        if (user == null)
        {
            return new DeleteUserResult { IsSuccess = false, Error = "User not found" };
        }

        // Fshirja reale
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return new DeleteUserResult { IsSuccess = true };
    }
}

public class DeleteUserResult
{
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }
}