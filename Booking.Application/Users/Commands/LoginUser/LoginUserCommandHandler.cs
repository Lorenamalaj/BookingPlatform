using Booking.Infrastructure.Data;
using Booking.Infrastructure.Authentication;
using Microsoft.EntityFrameworkCore;

namespace Booking.Application.Users.Commands.LoginUser;

public class LoginUserCommandHandler
{
    private readonly BookingPlatformDbContext _context;
    private readonly JwtService _jwtService;

    public LoginUserCommandHandler(BookingPlatformDbContext context, JwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<LoginUserResult> Handle(LoginUserCommand command)
    {
        // Find user by email
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == command.Email);

        if (user == null)
        {
            return new LoginUserResult
            {
                IsSuccess = false,
                Error = "Invalid email or password"
            };
        }

        // Verify password (simple Base64 for now - should use BCrypt)
        var passwordHash = Convert.ToBase64String(
            System.Text.Encoding.UTF8.GetBytes(command.Password)
        );

        if (user.PasswordHash != passwordHash)
        {
            return new LoginUserResult
            {
                IsSuccess = false,
                Error = "Invalid email or password"
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

        // Fallback: if no roles found but this is the seeded admin email, grant Admin role
        if (!userRoles.Any() && string.Equals(user.Email, "admin@bookingplatform.com", StringComparison.OrdinalIgnoreCase))
        {
            userRoles.Add("Admin");
        }

        // Generate JWT token
        var token = _jwtService.GenerateToken(user.Id, user.Email, userRoles);

        return new LoginUserResult
        {
            IsSuccess = true,
            Token = token,
            UserId = user.Id,
            Email = user.Email,
            Roles = userRoles
        };
    }
}

public class LoginUserResult
{
    public bool IsSuccess { get; set; }
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public Guid UserId { get; set; }
    public string? Email { get; set; }
    public List<string>? Roles { get; set; }
    public string? Error { get; set; }
}