using Booking.Infrastructure.Data;
using Booking.Infrastructure.Authentication;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Booking.Application.Users.Commands.RefreshToken;

public class RefreshTokenCommandHandler
{
    private readonly BookingPlatformDbContext _context;
    private readonly JwtService _jwtService;

    public RefreshTokenCommandHandler(BookingPlatformDbContext context, JwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<RefreshTokenResult> Handle(RefreshTokenCommand command)
    {
        // Validate refresh token exists and is valid
        var storedRefreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == command.RefreshToken);

        if (storedRefreshToken == null)
        {
            return new RefreshTokenResult
            {
                IsSuccess = false,
                Error = "Invalid refresh token"
            };
        }

        if (!storedRefreshToken.IsValid())
        {
            return new RefreshTokenResult
            {
                IsSuccess = false,
                Error = "Refresh token has expired or been revoked"
            };
        }

        // Get principal from expired access token
        ClaimsPrincipal? principal;
        try
        {
            principal = _jwtService.GetPrincipalFromExpiredToken(command.AccessToken);
        }
        catch
        {
            return new RefreshTokenResult
            {
                IsSuccess = false,
                Error = "Invalid access token"
            };
        }

        if (principal == null)
        {
            return new RefreshTokenResult
            {
                IsSuccess = false,
                Error = "Invalid token"
            };
        }

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            return new RefreshTokenResult
            {
                IsSuccess = false,
                Error = "Invalid token claims"
            };
        }

        var userId = Guid.Parse(userIdClaim.Value);

        // Verify refresh token belongs to this user
        if (storedRefreshToken.UserId != userId)
        {
            return new RefreshTokenResult
            {
                IsSuccess = false,
                Error = "Token mismatch"
            };
        }

        // Get user and roles
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return new RefreshTokenResult
            {
                IsSuccess = false,
                Error = "User not found"
            };
        }

        var userRoles = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Join(_context.Roles,
                ur => ur.RoleId,
                r => r.Id,
                (ur, r) => r.Name)
            .ToListAsync();

        // Generate new tokens
        var newAccessToken = _jwtService.GenerateToken(userId, user.Email, userRoles);
        var newRefreshToken = _jwtService.GenerateRefreshToken();

        // Revoke old refresh token
        storedRefreshToken.Revoke();

        // Save new refresh token
        var newRefreshTokenEntity = new Domain.RefreshTokens.RefreshToken(
            userId,
            newRefreshToken,
            DateTime.UtcNow.AddDays(7)
        );

        _context.RefreshTokens.Add(newRefreshTokenEntity);
        await _context.SaveChangesAsync();

        return new RefreshTokenResult
        {
            IsSuccess = true,
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken
        };
    }
}

public class RefreshTokenResult
{
    public bool IsSuccess { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public string? Error { get; set; }
}