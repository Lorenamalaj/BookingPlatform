using Booking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Booking.Application.Users.Queries.GetUser;

public class GetUserQueryHandler
{
    private readonly BookingPlatformDbContext _context;

    public GetUserQueryHandler(BookingPlatformDbContext context)
    {
        _context = context;
    }

    public async Task<GetUserResult> Handle(GetUserQuery query)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == query.UserId);

        if (user == null)
        {
            return new GetUserResult
            {
                IsSuccess = false,
                Error = "User not found"
            };
        }

        // Get user roles
        var userRoles = await _context.UserRoles
            .Where(ur => ur.UserId == user.Id)
            .Join(_context.Roles,
                ur => ur.RoleId,
                r => r.Id,
                (ur, r) => r.Name)
            .ToListAsync();

        var userDto = new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.isActive,
            CreatedAt = user.CreatedAt,
            LastModifiedAt = user.LastModifiedAt,
            Roles = userRoles
        };

        return new GetUserResult
        {
            IsSuccess = true,
            User = userDto
        };
    }
}

public class GetUserResult
{
    public bool IsSuccess { get; set; }
    public UserDto? User { get; set; }
    public string? Error { get; set; }
}

public class UserDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ProfileImageUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public List<string> Roles { get; set; } = new();
}