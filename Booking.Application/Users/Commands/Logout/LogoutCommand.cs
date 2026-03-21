namespace Booking.Application.Users.Commands.Logout;

public class LogoutCommand
{
    public string RefreshToken { get; set; } = string.Empty;
}