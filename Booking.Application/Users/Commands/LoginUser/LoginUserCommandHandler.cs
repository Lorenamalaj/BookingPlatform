using Booking.Infrastructure.Data;
using Booking.Infrastructure.Authentication;
using Booking.Domain.RefreshTokens;
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

        var passwordHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(command.Password));

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

        // Generate tokens
        var accessToken = _jwtService.GenerateToken(user.Id, user.Email, userRoles);
        var refreshToken = _jwtService.GenerateRefreshToken();

        // Save refresh token to database
        var refreshTokenEntity = new Booking.Domain.RefreshTokens.RefreshToken(
            user.Id,
            refreshToken,
            DateTime.UtcNow.AddDays(7)
        );

        _context.RefreshTokens.Add(refreshTokenEntity);
        await _context.SaveChangesAsync();

        return new LoginUserResult
        {
            IsSuccess = true,
            Token = accessToken,
            RefreshToken = refreshToken,
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